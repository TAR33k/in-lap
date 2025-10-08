import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, ReportDto } from './api.service';
import { Subscription } from 'rxjs';
import { trigger, transition, style, animate, state } from '@angular/animations';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  animations: [
    trigger('fadeSlide', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(8px)' }),
        animate('260ms 40ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'translateY(-6px)' }))
      ])
    ]),
    trigger('swapStates', [
      state('idle', style({ opacity: 1 })),
      state('loading', style({ opacity: 1 })),
      state('done', style({ opacity: 1 })),
      state('error', style({ opacity: 1 })),
      transition('idle => loading, loading => done, error => idle, * => error', [
        style({ opacity: 0, transform: 'scale(0.98)' }),
        animate('220ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ],
  template: `
  <div class="page">
    <div class="bg"></div>
    <div class="bg-overlay"></div>

    <header class="header container">
      <div class="brand">
        <h1><span class="brand-in">In</span><span class="brand-lap">Lap</span> – AI Motorsport Journalist</h1>
      </div>
      <nav class="links">
        <a href="https://github.com/TAR33k/in-lap" target="_blank" rel="noopener" aria-label="GitHub" title="GitHub">
          <svg viewBox="0 0 16 16" width="22" height="22" fill="currentColor" focusable="false" aria-hidden="true">
            <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"/>
          </svg>
        </a>
      </nav>
    </header>

    <main class="container main" [@swapStates]="state">
      <section class="panel upload" *ngIf="state === 'idle' || state === 'error'" @fadeSlide>
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

      <section class="panel loading" *ngIf="state === 'loading'" @fadeSlide>
        <div class="loader"><div class="bar" [style.width.%]="progress"></div></div>
        <div class="loading-text">Generating motorsport article… {{ progress }}%</div>
        <div class="speedlines" aria-hidden="true">
          <div class="layer l1"></div>
          <div class="layer l2"></div>
          <div class="layer l3"></div>
          <div class="glow"></div>
          <div class="sparks"></div>
        </div>
      </section>

      <section class="panel result" *ngIf="state === 'done' && report" @fadeSlide>
        <div class="result-grid">
          <article class="article">
            <h2 class="title" *ngIf="article.title">{{ article.title }}</h2>
            <p class="lead" *ngIf="article.lead">{{ article.lead }}</p>
            <div class="divider" aria-hidden="true"></div>
            <div class="body">
              <p *ngFor="let p of article.body">{{ p }}</p>
            </div>
            <div class="divider faint" *ngIf="article.quickFacts?.length" aria-hidden="true"></div>
            <section class="quickfacts" *ngIf="article.quickFacts?.length">
              <h3>Quick facts</h3>
              <ul>
                <li *ngFor="let q of article.quickFacts">{{ q }}</li>
              </ul>
            </section>

            <section class="race-results" *ngIf="raceResults?.length">
              <div class="race" *ngFor="let race of raceResults">
                <h3>{{ race.label }}</h3>
                <div class="table">
                  <div class="thead">
                    <div>Pos</div>
                    <div>Driver</div>
                    <div>Gap</div>
                  </div>
                  <div class="tbody">
                    <div class="row" *ngFor="let row of race.rows">
                      <div class="pos">{{ row.pos }}</div>
                      <div class="driver">{{ row.driver }}</div>
                      <div class="gap">{{ row.gap || '—' }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </section>
          </article>

          <aside class="side" *ngIf="highlights as h">
            <div class="side-card">
              <h4>Weekend summary</h4>
              <div class="kv" *ngIf="h.track"><span>Track</span><strong>{{ h.track }}</strong></div>
              <div class="kv" *ngIf="h.game"><span>Game</span><strong>{{ h.game }}</strong></div>
              <div class="kv" *ngIf="h.date"><span>Date</span><strong>{{ h.date }}</strong></div>
              <div class="kv" *ngIf="h.pole"><span>Pole</span><strong>{{ h.pole }}</strong></div>
              <div class="kv" *ngIf="h.race1"><span>Race 1</span><strong>{{ h.race1 }}</strong></div>
              <div class="kv" *ngIf="h.race2"><span>Race 2</span><strong>{{ h.race2 }}</strong></div>
            </div>
            <div class="side-actions">
              <button class="btn" (click)="copyArticle()">{{ copied ? 'Copied!' : 'Copy Article' }}</button>
              <button class="btn accent" (click)="reset()">Generate Another</button>
            </div>
          </aside>
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
  article: { title: string; lead: string; body: string[]; quickFacts: string[] } = { title: '', lead: '', body: [], quickFacts: [] };
  raceResults: { label: string; rows: { pos: number; driver: string; gap?: string }[] }[] = [];
  copied = false;
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
    this.article = { title: '', lead: '', body: [], quickFacts: [] };
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
    this.article = this.parseArticle(r.article || '');
    this.highlights = this.computeHighlights(r.summaryJson);
    this.raceResults = this.computeRaceResults(r.summaryJson);
  }

  private parseArticle(raw: string): { title: string; lead: string; body: string[]; quickFacts: string[] } {
    const lines = raw.split(/\n+/).map(l => l.trim()).filter(Boolean);
    let title = '';
    let lead = '';
    const quickFacts: string[] = [];
    const content: string[] = [];

    const strip = (s: string, key: string) => s.replace(new RegExp(`^${key}\\s*[—:-]\\s*`, 'i'), '').trim();
    const isTagOnly = (s: string) => /^(headline|lead|body|quick\s*facts)\s*[—:-]?\s*$/i.test(s);

    let mode: 'body' | 'facts' = 'body';
    const norm = (s: string) => s.replace(/\s+/g, ' ').trim().toLowerCase();
    for (const l of lines) {
      if (isTagOnly(l)) {
        if (/^quick\s*facts/i.test(l)) mode = 'facts'; else if (/^body/i.test(l)) mode = 'body';
        continue;
      }
      if (/^headline\b/i.test(l)) { const t = strip(l, 'headline'); if (t) title = t; continue; }
      if (/^lead\b/i.test(l)) { const ld = strip(l, 'lead'); if (ld) lead = ld; continue; }
      if (/^body\b/i.test(l)) { mode = 'body'; continue; }
      if (/^quick\s+facts\b/i.test(l)) { mode = 'facts'; continue; }
      if (mode === 'facts') {
        const fact = l.replace(/^[−\-•\s]+/, '').trim();
        if (fact) quickFacts.push(fact);
      } else {
        if (l) content.push(l);
      }
    }
    if (!title && content.length) { title = content[0]; }
    if (!lead && content.length > 1) { lead = content[1]; }
    if (lead && title && norm(lead) === norm(title)) { lead = ''; }
    const body = content.filter(l => norm(l) !== norm(title) && (lead ? norm(l) !== norm(lead) : true));

    return { title, lead, body, quickFacts };
  }

  copyArticle() {
    const text = this.buildArticleText();
    if (navigator?.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        this.copied = true;
        setTimeout(() => this.copied = false, 1500);
      }).catch(() => { });
    }
  }

  private buildArticleText(): string {
    const parts: string[] = [];
    if (this.article.title) parts.push(this.article.title);
    if (this.article.lead) parts.push(this.article.lead);
    if (this.article.body?.length) parts.push(...this.article.body);
    if (this.article.quickFacts?.length) {
      parts.push('Quick facts');
      for (const q of this.article.quickFacts) parts.push(`- ${q}`);
    }
    if (this.raceResults?.length) {
      for (const race of this.raceResults) {
        parts.push(race.label);
        parts.push('Pos  Driver  Gap');
        for (const row of race.rows) {
          parts.push(`${row.pos}  ${row.driver}  ${row.gap || '—'}`);
        }
      }
    }
    return parts.join('\n');
  }

  private computeRaceResults(summaryJson: string): { label: string; rows: { pos: number; driver: string; gap?: string }[] }[] {
    try {
      const s = JSON.parse(summaryJson);
      const sessions = (s.sessions as any[]) || [];
      const collect = (type: string, label: string) => {
        const ss = sessions.find(x => (x.type || x.Type) === type);
        const arr = (ss?.results || ss?.topFinishers || []) as any[];
        const rows = arr
          .map((r: any) => ({ pos: Number(r.pos ?? r.position ?? r.Place), driver: r.driver ?? r.Driver ?? r.name, gap: r.gap ?? r.Gap }))
          .filter(x => Number.isFinite(x.pos) && x.driver);
        return rows.length ? { label, rows } : null;
      };
      return [collect('Race1', 'Race 1 Top 10'), collect('Race2', 'Race 2 Top 10')].filter(Boolean) as any;
    } catch {
      return [];
    }
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
        pole: pole,
        race1: race1,
        race2: race2,
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
