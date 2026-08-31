import { useState } from 'react';
import { useForm } from '@mantine/form';
import { Button, PasswordInput, Stack, TextInput } from '@mantine/core';
import { asApiError } from '../../api/problemDetails';
import type { ApiError } from '../../api/problemDetails';
import { useAuth } from '../../auth/useAuth';
import { validatePassword, validateStoreId } from '../../validation/credentials';
import { ProblemAlert } from '../../components/ProblemAlert';

/** The inputs this form binds ModelState keys to; see ProblemAlert. */
const FIELDS = ['storeId', 'password'] as const;

interface LoginFormValues {
  storeId: string;
  password: string;
}

export function LoginForm({ onSuccess }: { onSuccess: () => void }) {
  const { signIn } = useAuth();
  const [error, setError] = useState<ApiError | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const form = useForm<LoginFormValues>({
    mode: 'uncontrolled',
    initialValues: { storeId: '', password: '' },
    validate: {
      storeId: validateStoreId,
      password: validatePassword,
    },
  });

  const handleSubmit = async (values: LoginFormValues) => {
    setError(null);
    setSubmitting(true);

    try {
      await signIn(values);
      onSuccess();
    } catch (cause) {
      const apiError = asApiError(cause);
      setError(apiError);

      // Attach server-side validation messages to the inputs they belong to.
      // Fields the server flagged that we do not render are shown by
      // ProblemAlert instead.
      const serverErrors: Record<string, string> = {};
      for (const field of FIELDS) {
        const messages = apiError.errorsFor(field);
        if (messages.length > 0) {
          serverErrors[field] = messages.join(' ');
        }
      }

      if (Object.keys(serverErrors).length > 0) {
        form.setErrors(serverErrors);
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={form.onSubmit((values) => void handleSubmit(values))} noValidate>
      <Stack gap="md">
        {error !== null && <ProblemAlert error={error} handledFields={FIELDS} />}

        <TextInput
          label="Store id"
          placeholder="demo-store"
          autoComplete="username"
          autoFocus
          key={form.key('storeId')}
          {...form.getInputProps('storeId')}
        />

        <PasswordInput
          label="Password"
          autoComplete="current-password"
          key={form.key('password')}
          {...form.getInputProps('password')}
        />

        <Button type="submit" loading={submitting} fullWidth>
          Sign in
        </Button>
      </Stack>
    </form>
  );
}
