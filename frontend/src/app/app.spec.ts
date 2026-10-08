import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App (página de status)', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      // provideHttpClientTesting troca o HTTP real por um "falso" que o teste controla.
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Garante que nenhuma requisição ficou sem resposta (ou foi feita sem querer).
    http.verify();
  });

  async function render() {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges(); // dispara o ngOnInit
    return fixture;
  }

  it('mostra API e banco OK quando os dois respondem', async () => {
    const fixture = await render();

    http.expectOne('/api/ping').flush({ message: 'pong', serverTime: '2026-10-08T12:00:00Z' });
    http.expectOne('/health/ready').flush({ status: 'Healthy', checks: { database: 'Healthy' } });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Respondeu "pong"');
    expect(text).toContain('Conectado ao PostgreSQL');
  });

  it('mostra banco indisponível quando /health/ready responde 503', async () => {
    const fixture = await render();

    http.expectOne('/api/ping').flush({ message: 'pong', serverTime: '2026-10-08T12:00:00Z' });
    http
      .expectOne('/health/ready')
      .flush(
        { status: 'Unhealthy', checks: { database: 'Unhealthy' } },
        { status: 503, statusText: 'Service Unavailable' },
      );
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Banco indisponível');
  });

  it('mostra erro quando a API não responde', async () => {
    const fixture = await render();

    http.expectOne('/api/ping').error(new ProgressEvent('network error'));
    http.expectOne('/health/ready').error(new ProgressEvent('network error'));
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Sem resposta da API');
  });
});
