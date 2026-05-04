import {useState} from 'react';
import {handleAcceptLegal, handleRejectLegal} from '@/app/(auth)/utils';

export function useLegalAcceptance() {
  const [legalAccepted, setLegalAccepted] = useState({
    terms: false,
    privacy: false,
  });

  const onAccept = (type: string) => {
    handleAcceptLegal(type);
    setLegalAccepted(prev => ({...prev, [type]: true}));
  };

  const onReject = (type: string) => {
    handleRejectLegal(type);
    setLegalAccepted(prev => ({...prev, [type]: false}));
  };

  const isLegalComplete = legalAccepted.terms && legalAccepted.privacy;

  return {legalAccepted, onAccept, onReject, isLegalComplete};
}
