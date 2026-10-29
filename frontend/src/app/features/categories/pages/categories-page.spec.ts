import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Category } from '../data/category.models';
import { CategoriesPage } from './categories-page';

const category = (id: string, name: string, type: Category['type']): Category => ({
  id,
  name,
  type,
  color: '#000000',
  icon: 'more_horiz',
  isArchived: false,
  transactionCount: 0,
});

describe('CategoriesPage', () => {
  let fixture: ComponentFixture<CategoriesPage>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoriesPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(CategoriesPage);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  const text = () => (fixture.nativeElement as HTMLElement).textContent ?? '';

  it('separa despesas e receitas nas abas, com a contagem de cada uma', async () => {
    http.expectOne('/api/categories?includeArchived=false').flush([
      category('1', 'Alimentação', 'Expense'),
      category('2', 'Moradia', 'Expense'),
      category('3', 'Salário', 'Income'),
    ]);
    await fixture.whenStable();

    expect(text()).toContain('Despesas (2)');
    expect(text()).toContain('Receitas (1)');
    // A aba aberta por padrão é a de despesas.
    expect(text()).toContain('Alimentação');
    expect(text()).toContain('Moradia');
  });

  it('a aba sem categorias mostra o estado vazio', async () => {
    http.expectOne('/api/categories?includeArchived=false').flush([category('3', 'Salário', 'Income')]);
    await fixture.whenStable();

    expect(text()).toContain('Despesas (0)');
    expect(text()).toContain('Nenhuma categoria aqui');
  });
});
