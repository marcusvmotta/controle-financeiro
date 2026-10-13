import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { DashboardPage } from './dashboard-page';

describe('DashboardPage (status do sistema)', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function render() {
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges(); // dispara o ngOnInit
    return fixture;
  }

  it('mostra API e banco OK quando os dois respondem', async () => {
    const fixture = await render();

    http.expectOne('/api/ping').flush({ message: 'pong', serverTime: '2026-10-13T12:00:00Z' });
    http.expectOne('/health/ready').flush({ status: 'Healthy', checks: { database: 'Healthy' } });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Respondeu "pong"');
    expect(text).toContain('Conectado ao PostgreSQL');
  });

  it('mostra banco indisponível quando /health/ready responde 503', async () => {
    const fixture = await render();

    http.expectOne('/api/ping').flush({ message: 'pong', serverTime: '2026-10-13T12:00:00Z' });
    http
      .expectOne('/health/ready')
      .flush(
        { status: 'Unhealthy', checks: { database: 'Unhealthy' } },
        { status: 503, statusText: 'Service Unavailable' },
      );
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Banco indisponível');
  });
});
