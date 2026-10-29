import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { AccountRequest } from '../data/account.models';
import { AccountDialog, AccountDialogData } from './account-dialog';

describe('AccountDialog', () => {
  let fixture: ComponentFixture<AccountDialog>;
  let save: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;

  async function render(data: Partial<AccountDialogData> = {}) {
    save = vi.fn().mockReturnValue(of({}));
    close = vi.fn();
    await TestBed.configureTestingModule({
      imports: [AccountDialog],
      providers: [
        // O diálogo de verdade é aberto pelo MatDialog; no teste, entregamos os dados e o "ref" à mão.
        { provide: MAT_DIALOG_DATA, useValue: { save, ...data } },
        { provide: MatDialogRef, useValue: { close } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(AccountDialog);
    await fixture.whenStable();
  }

  const page = () => fixture.nativeElement as HTMLElement;

  function type(selector: string, value: string): void {
    const input = page().querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    page().querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  it('converte o valor brasileiro antes de enviar e fecha ao salvar', async () => {
    await render();
    type('input[formControlName=name]', '  Nubank ');
    type('input[formControlName=initialBalance]', '1.234,56');
    await submit();

    const request = save.mock.calls[0][0] as AccountRequest;
    expect(request.name).toBe('Nubank');
    expect(request.initialBalance).toBe(1234.56);
    expect(close).toHaveBeenCalledWith(true);
  });

  it('não envia com valor inválido e explica o formato', async () => {
    await render();
    type('input[formControlName=name]', 'Nubank');
    type('input[formControlName=initialBalance]', '10,005');
    await submit();

    expect(save).not.toHaveBeenCalled();
    expect(page().textContent).toContain('Valor inválido');
  });

  it('mostra o conflito da API dentro do diálogo, sem fechar', async () => {
    await render();
    save.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Conflito', status: 409, detail: 'Você já tem uma conta chamada "Nubank".' },
          }),
      ),
    );
    type('input[formControlName=name]', 'Nubank');
    await submit();

    expect(page().querySelector('[role=alert]')?.textContent).toContain('Você já tem uma conta chamada');
    expect(close).not.toHaveBeenCalled();
  });

  it('na edição, preenche o formulário com o saldo no formato brasileiro', async () => {
    await render({
      account: {
        id: '1',
        name: 'Cartão',
        type: 'CreditCard',
        initialBalance: -320.1,
        currentBalance: -320.1,
        color: '#000000',
        icon: 'credit_card',
        isArchived: false,
      },
    });

    const balance = page().querySelector<HTMLInputElement>('input[formControlName=initialBalance]')!;
    expect(balance.value).toBe('-320,10');
    expect(page().textContent).toContain('Editar conta');
  });
});
