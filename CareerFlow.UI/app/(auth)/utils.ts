import { Alert } from "react-native";

  export  function handleAcceptLegal(type: string):void {
    if (type === 'privacy') {
      console.log('Utilizatorul a acceptat Politica de Confidențialitate');
    } else if (type === 'terms') {
      console.log('Utilizatorul a acceptat Termenii și Condițiile');
    }
  };

   export  function handleRejectLegal(type: string):void {
    console.log(`Utilizatorul a refuzat: ${type}`);
    const documentName = type === 'privacy' 
      ? 'Politicii de Confidențialitate' 
      : 'Termenilor și Condițiilor';
      
    Alert.alert('Notă', `Acceptarea ${documentName} este necesară pentru accesul complet.`);
  };