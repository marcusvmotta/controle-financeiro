import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Converte o texto digitado pelo usuário em número (TRD 05-frontend, "Formatação").
 * Aceita o jeito brasileiro ("1.234,56", "1234,5", "-320,10") e também "1234.56".
 * Devolve null se o texto não for um valor válido com no máximo 2 casas decimais.
 */
export function parseMoney(text: string): number | null {
  const value = text.trim().replace(/^R\$\s*/, '');
  if (value === '') {
    return null;
  }

  // Com vírgula: pontos são separadores de milhar e a vírgula é decimal ("1.234,56").
  // Sem vírgula: aceita um ponto como decimal ("1234.56"), mas não milhar ambíguo ("1.234").
  const normalized = value.includes(',')
    ? /^-?\d{1,3}(\.\d{3})*,\d{1,2}$|^-?\d+,\d{1,2}$/.test(value)
      ? value.replace(/\./g, '').replace(',', '.')
      : null
    : /^-?\d+(\.\d{1,2})?$/.test(value)
      ? value
      : null;

  return normalized === null ? null : Number(normalized);
}

/** Formata um número para edição no campo: 1500.5 → "1500,50" (sem separador de milhar, mais fácil de editar). */
export function formatMoneyForInput(value: number): string {
  return value.toFixed(2).replace('.', ',');
}

/** Validador de formulário: o texto precisa ser um valor monetário válido. */
export const moneyValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const text = String(control.value ?? '');
  return text.trim() === '' || parseMoney(text) !== null ? null : { money: true };
};
