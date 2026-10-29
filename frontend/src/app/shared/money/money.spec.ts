import { formatMoneyForInput, parseMoney } from './money';

describe('parseMoney', () => {
  it.each([
    ['1500', 1500],
    ['1500,5', 1500.5],
    ['1500,50', 1500.5],
    ['1.234,56', 1234.56],
    ['1.234.567,89', 1234567.89],
    ['-320,10', -320.1],
    ['1234.56', 1234.56],
    ['  R$ 10,00 ', 10],
    ['0', 0],
  ])('"%s" → %d', (text, expected) => {
    expect(parseMoney(text)).toBe(expected);
  });

  it.each([
    ['', 'vazio'],
    ['abc', 'texto'],
    ['10,005', 'três casas decimais'],
    ['1.234', 'milhar sem vírgula é ambíguo'],
    ['1,2,3', 'duas vírgulas'],
    ['12.34.56', 'pontos sem sentido'],
  ])('"%s" é inválido (%s)', (text) => {
    expect(parseMoney(text)).toBeNull();
  });
});

describe('formatMoneyForInput', () => {
  it('usa vírgula e sempre 2 casas', () => {
    expect(formatMoneyForInput(1500.5)).toBe('1500,50');
    expect(formatMoneyForInput(-320.1)).toBe('-320,10');
  });
});
