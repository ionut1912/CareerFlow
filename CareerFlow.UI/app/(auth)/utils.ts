import {Alert} from 'react-native';

export function handleAcceptLegal(type: string): void {
  Alert.alert(
    'Mulțumim!',
    `Ați acceptat ${type === 'privacy' ? 'Politica de Confidențialitate' : 'Termenii și Condițiile'}.`,
  );
}

export function handleRejectLegal(type: string): void {
  const documentName =
    type === 'privacy'
      ? 'Politicii de Confidențialitate'
      : 'Termenilor și Condițiilor';

  Alert.alert(
    'Notă',
    `Acceptarea ${documentName} este necesară pentru accesul complet.`,
  );
}
