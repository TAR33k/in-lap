import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { EMPTY, Observable, throwError, timer } from 'rxjs';
import { catchError, map, switchMap, take, timeout as rxTimeout } from 'rxjs/operators';
import { environment } from '../environments/environment';

export interface UploadResponseDto {
  uploadId: string;
}

export interface ReportDto {
  uploadId: string;
  summaryJson: string;
  article: string;
  llmRaw: string;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = this.resolveApiBase();

  constructor(private http: HttpClient) {}

  private resolveApiBase(): string {
    const api = (environment as any)?.API_URL ?? '';
    if (api && api.trim().length > 0) return api.replace(/\/$/, '');
    const loc = window.location;
    return `${loc.protocol}//${loc.host}`;
  }

  uploadCsv(file: File): Observable<UploadResponseDto> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<UploadResponseDto>(`${this.baseUrl}/api/upload`, form).pipe(
      catchError(this.handleError)
    );
  }

  getReport(uploadId: string): Observable<ReportDto> {
    return this.http.get<ReportDto>(`${this.baseUrl}/api/reports/${uploadId}`).pipe(
      catchError(this.handleError)
    );
  }

  pollReport(uploadId: string, intervalMs = 1200, maxAttempts = 60): Observable<ReportDto> {
    const overallMs = intervalMs * Math.max(1, maxAttempts);
    return timer(0, intervalMs).pipe(
      switchMap(() => this.getReport(uploadId).pipe(
        catchError((err: HttpErrorResponse) => err.status === 404 ? EMPTY : throwError(() => err))
      )),
      take(1),
      rxTimeout({ each: overallMs + intervalMs })
    );
  }

  private handleError(err: HttpErrorResponse) {
    return throwError(() => err);
  }
}
