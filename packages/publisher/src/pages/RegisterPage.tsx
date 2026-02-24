import { RegistrationWizard } from '@/features/Registration';

export function RegisterPage() {
  return (
    <div className="page page--register">
      <header className="register-header">
        <div>
          <h1>Register Story</h1>
          <p className="register-header__subtitle">
            Follow the steps below to register your story on-chain with transparent attribution
          </p>
        </div>
      </header>

      <RegistrationWizard />
    </div>
  );
}
