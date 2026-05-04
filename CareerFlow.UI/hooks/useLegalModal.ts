import {getLegal} from '@/services/legalService';
import {useState} from 'react';

interface ModalState {
  visible: boolean;
  loading: boolean;
  title: string;
  content: string;
  type: string;
}

const INITIAL_MODAL_STATE: ModalState = {
  visible: false,
  loading: false,
  title: '',
  content: '',
  type: '',
};

const LEGAL_TITLES: Record<string, string> = {
  privacy: 'Politica de Confidențialitate',
  terms: 'Termeni și Condiții',
};

export function useLegalModal(
  onAccept?: (type: string) => void,
  onReject?: (type: string) => void,
) {
  const [modal, setModal] = useState<ModalState>(INITIAL_MODAL_STATE);

  const open = async (type: 'privacy' | 'terms') => {
    setModal(prev => ({...prev, visible: true, loading: true, type}));
    try {
      const res = await getLegal(type);
      setModal(prev => ({
        ...prev,
        loading: false,
        title: LEGAL_TITLES[type],
        content: res.data.content,
      }));
    } catch {
      setModal(prev => ({
        ...prev,
        loading: false,
        title: 'Eroare',
        content: 'Eroare la încărcarea datelor.',
      }));
    }
  };

  const close = () => setModal(prev => ({...prev, visible: false}));

  const handleAccept = () => {
    onAccept?.(modal.type);
    close();
  };

  const handleReject = () => {
    onReject?.(modal.type);
    close();
  };

  return {modal, open, close, handleAccept, handleReject};
}
