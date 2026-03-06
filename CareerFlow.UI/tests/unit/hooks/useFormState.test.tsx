import {useFormState} from '@/hooks/useFormState';
import {renderHook, act} from '@testing-library/react-native';

describe('useFormState', () => {
  const initialState = {
    username: 'john_doe',
    email: 'john@example.com',
  };

  it('initializes with the correct form and untouched states', () => {
    const {result} = renderHook(() => useFormState(initialState));

    expect(result.current.form).toEqual(initialState);
    expect(result.current.touched).toEqual({
      username: false,
      email: false,
    });
  });

  it('updates a specific field value on handleChange without mutating others', () => {
    const {result} = renderHook(() => useFormState(initialState));

    act(() => {
      // Testing the curried function signature
      result.current.handleChange('username')('jane_doe');
    });

    expect(result.current.form.username).toBe('jane_doe');
    expect(result.current.form.email).toBe('john@example.com');
  });

  it('marks a specific field as touched on handleBlur without affecting others', () => {
    const {result} = renderHook(() => useFormState(initialState));

    act(() => {
      // Testing the curried function signature
      result.current.handleBlur('email')();
    });

    expect(result.current.touched.email).toBe(true);
    expect(result.current.touched.username).toBe(false);
  });

  it('marks all fields as touched when touchAll is called', () => {
    const {result} = renderHook(() => useFormState(initialState));

    act(() => {
      result.current.touchAll();
    });

    expect(result.current.touched).toEqual({
      username: true,
      email: true,
    });
  });

  it('handles multiple state updates accurately over time', () => {
    const {result} = renderHook(() => useFormState(initialState));

    act(() => {
      result.current.handleChange('username')('super_admin');
      result.current.handleBlur('username')();
    });

    expect(result.current.form.username).toBe('super_admin');
    expect(result.current.touched.username).toBe(true);

    // Ensure email remains completely untouched and unchanged
    expect(result.current.form.email).toBe('john@example.com');
    expect(result.current.touched.email).toBe(false);
  });
});
