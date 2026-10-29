/**
 * Cores e ícones disponíveis para contas e categorias.
 * Os ícones precisam estar na lista permitida do backend
 * (backend: Application/Common/Validation/AppearanceRules.cs), senão a API recusa com 400.
 */

export const COLOR_PALETTE: readonly string[] = [
  '#8A05BE', '#3F51B5', '#0288D1', '#00BCD4', '#2E7D32', '#4CAF50',
  '#F9A825', '#FF9800', '#E91E63', '#C62828', '#795548', '#607D8B',
];

export const ACCOUNT_ICONS: readonly string[] = [
  'account_balance', 'account_balance_wallet', 'savings', 'credit_card', 'payments', 'wallet',
  'currency_exchange', 'attach_money', 'trending_up',
];

export const CATEGORY_ICONS: readonly string[] = [
  'restaurant', 'local_cafe', 'home', 'directions_car', 'local_gas_station', 'directions_bus',
  'medical_services', 'fitness_center', 'sports_esports', 'movie', 'school', 'menu_book',
  'shopping_bag', 'shopping_cart', 'subscriptions', 'pets', 'child_care', 'checkroom', 'flight',
  'work', 'payments', 'savings', 'card_giftcard', 'receipt_long', 'bolt', 'water_drop', 'wifi',
  'phone_iphone', 'more_horiz',
];
