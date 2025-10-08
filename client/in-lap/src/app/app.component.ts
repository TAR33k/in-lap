import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, ReportDto } from './api.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="page">
    <div class="bg"></div>
    <div class="bg-overlay"></div>

    <header class="header container">
      <div class="brand">
        <h1>In-Lap – AI Motorsport Journalist</h1>
      </div>
      <nav class="links">
        <a href="https://github.com/TAR33k/in-lap" target="_blank" rel="noopener" aria-label="GitHub" title="GitHub">
          <svg viewBox="0 0 16 16" width="22" height="22" fill="currentColor" focusable="false" aria-hidden="true">
            <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"/>
          </svg>
        </a>
      </nav>
    </header>

    <main class="container main">
      <section class="panel upload" *ngIf="state === 'idle' || state === 'error'">
        <div class="upload-inner" (drop)="onDrop($event)" (dragover)="onDragOver($event)">
          <div class="upload-icon">📄</div>
          <div class="upload-title">Upload race weekend CSV</div>
          <div class="upload-sub">Drag & drop your file here, or choose from your computer</div>

          <div class="picker">
            <input type="file" accept=".csv" (change)="onFileInputChange($event)" id="fileInput" hidden>
            <label class="btn" for="fileInput">Choose File</label>
            <button class="btn accent" [disabled]="!selectedFile" (click)="generate()">Generate Report</button>
          </div>

          <div class="chosen" *ngIf="selectedFile">
            <span class="file-name">{{ selectedFile.name }}</span>
            <button class="link" (click)="removeFile()">Remove</button>
          </div>

          <div class="error" *ngIf="state === 'error'">{{ errorText }}</div>
        </div>
      </section>

      <section class="panel loading" *ngIf="state === 'loading'">
        <div class="loader"><div class="bar" [style.width.%]="progress"></div></div>
        <div class="loading-text">Generating clean motorsport article… {{ progress }}%</div>
      </section>

      <section class="panel result" *ngIf="state === 'done' && report">
        <div class="headline" *ngIf="articleLines[0] as head">{{ head.replace('HEADLINE —','').trim() }}</div>

        <div class="highlights" *ngIf="highlights as h">
          <div class="card">
            <div class="row">
              <div class="kv"><span>Track</span><strong>{{ h.track || '—' }}</strong></div>
              <div class="kv"><span>Game</span><strong>{{ h.game || '—' }}</strong></div>
              <div class="kv"><span>Date</span><strong>{{ h.date || '—' }}</strong></div>
            </div>
            <div class="row">
              <div class="kv"><span>Pole</span><strong>{{ h.pole || '—' }}</strong></div>
              <div class="kv"><span>Race 1</span><strong>{{ h.race1 || '—' }}</strong></div>
              <div class="kv"><span>Race 2</span><strong>{{ h.race2 || '—' }}</strong></div>
            </div>
          </div>
        </div>

        <article class="article">
          <div *ngFor="let line of articleLines">
            {{ line }}
          </div>
        </article>

        <div class="actions">
          <button class="btn" (click)="reset()">Generate Another</button>
        </div>
      </section>
    </main>
  </div>
  `,
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnDestroy {
  title = 'In-Lap – AI Motorsport Journalist';

  state: 'idle' | 'loading' | 'done' | 'error' = 'idle';
  errorText = '';
  progress = 0;

  selectedFile: File | null = null;
  report: ReportDto | null = null;
  articleLines: string[] = [];
  highlights: {
    track?: string;
    game?: string;
    date?: string;
    pole?: string;
    race1?: string;
    race2?: string;
  } = {};

  constructor(private api: ApiService) {}

  private pollSub?: Subscription;

  onFileInputChange(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.setFile(file);
  }

  reset() {
    this.pollSub?.unsubscribe();
    this.state = 'idle';
    this.selectedFile = null;
    this.report = null;
    this.articleLines = [];
    this.errorText = '';
    this.progress = 0;
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  onDrop(e: DragEvent) {
    e.preventDefault();
    const file = e.dataTransfer?.files?.[0] ?? null;
    this.setFile(file);
  }

  onDragOver(e: DragEvent) { e.preventDefault(); }

  removeFile() {
    this.selectedFile = null;
  }

  generate() {
    if (!this.selectedFile) return;

    this.pollSub?.unsubscribe();
    this.state = 'loading';
    this.errorText = '';
    this.progress = 5;

    const tick = () => {
      if (this.state === 'loading') {
        this.progress = Math.min(95, this.progress + 3);
        requestAnimationFrame(tick);
      }
    };
    requestAnimationFrame(tick);

    const file = this.selectedFile;
    this.api.uploadCsv(file).subscribe({
      next: ({ uploadId }) => {
        this.pollSub = this.api.pollReport(uploadId, 1200, 90).subscribe({
          next: (r) => {
            this.report = r;
            this.progress = 100;
            this.state = 'done';
            this.prepareResult(r);
            this.pollSub?.unsubscribe();
          },
          error: (err) => this.fail(err)
        });
      },
      error: (err) => this.fail(err)
    });
  }

  private setFile(file: File | null) {
    if (file && !file.name.toLowerCase().endsWith('.csv')) {
      this.errorText = 'Only .csv files are allowed';
      this.selectedFile = null;
      return;
    }
    this.errorText = '';
    this.selectedFile = file;
  }

  private prepareResult(r: ReportDto) {
    this.articleLines = (r.article || '').split(/\n+/).filter(Boolean);
    this.highlights = this.computeHighlights(r.summaryJson);
  }

  private computeHighlights(summaryJson: string) {
    try {
      const s = JSON.parse(summaryJson);
      const sessions = s.sessions as any[] || [];
      const weekend = s.weekend || {};
      const pole = this.findTop(sessions, 'Qualify');
      const race1 = this.findTop(sessions, 'Race1');
      const race2 = this.findTop(sessions, 'Race2');
      return {
        track: weekend.track,
        game: weekend.game,
        date: weekend.date,
        pole: pole ?? '—',
        race1: race1 ?? '—',
        race2: race2 ?? '—',
      };
    } catch {
      return {};
    }
  }

  private findTop(sessions: any[], type: string): string | undefined {
    const ss = sessions.find(x => (x.type || x.Type) === type);
    const tf = ss?.topFinishers?.find((t: any) => t.pos === 1);
    return tf?.driver;
  }

  private fail(err: any) {
    console.error(err);
    this.state = 'error';
    this.errorText = this.extractError(err);
    this.pollSub?.unsubscribe();
  }

  private extractError(err: any): string {
    if (!err) return 'Unexpected error';
    if (err.error?.title) return err.error.title;
    if (err.status === 413) return 'Upload too large (max ~1MB).';
    return err.message || 'Request failed';
  }
}
