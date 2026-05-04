import {useState} from 'react';

export function useFormState<T extends Record<string, string>>(initial: T) {
  const [form, setForm] = useState<T>(initial);
  const [touched, setTouched] = useState<Record<keyof T, boolean>>(
    () =>
      Object.fromEntries(Object.keys(initial).map(k => [k, false])) as Record<
        keyof T,
        boolean
      >,
  );

  const handleChange =
    (field: keyof T) =>
    (value: string): void =>
      setForm(prev => ({...prev, [field]: value}));

  const handleBlur = (field: keyof T) => (): void =>
    setTouched(prev => ({...prev, [field]: true}));

  const touchAll = () =>
    setTouched(
      Object.fromEntries(Object.keys(form).map(k => [k, true])) as Record<
        keyof T,
        boolean
      >,
    );

  return {form, touched, handleChange, handleBlur, touchAll};
}
