import { useState } from 'react';
import type { Story } from '@/api/types';
import { Button, Card, CardBody, CardFooter } from '@/components';
import { StoryPicker } from './StoryPicker';
import { ContributorManager } from '@/features/Contributor';
import { RoyaltySplitEditor } from '@/features/Contributor';
import { RegistrationStatus } from './RegistrationStatus';
import { useRegistration } from '../hooks/useRegistration';

type WizardStep = 'select' | 'contributors' | 'splits' | 'review' | 'register';

const STEPS: { id: WizardStep; label: string }[] = [
  { id: 'select', label: 'Select Story' },
  { id: 'contributors', label: 'Contributors' },
  { id: 'splits', label: 'Royalty Splits' },
  { id: 'review', label: 'Review' },
  { id: 'register', label: 'Register' },
];

export function RegistrationWizard() {
  const [currentStep, setCurrentStep] = useState<WizardStep>('select');
  const [selectedStory, setSelectedStory] = useState<Story | null>(null);
  const { register, isRegistering, registrationResult } = useRegistration();

  const currentStepIndex = STEPS.findIndex(s => s.id === currentStep);

  const goToStep = (step: WizardStep) => {
    setCurrentStep(step);
  };

  const goNext = () => {
    const nextIndex = currentStepIndex + 1;
    if (nextIndex < STEPS.length) {
      setCurrentStep(STEPS[nextIndex]!.id);
    }
  };

  const goBack = () => {
    const prevIndex = currentStepIndex - 1;
    if (prevIndex >= 0) {
      setCurrentStep(STEPS[prevIndex]!.id);
    }
  };

  const handleStorySelect = (story: Story) => {
    setSelectedStory(story);
  };

  const handleRegister = async () => {
    if (!selectedStory) return;
    await register(selectedStory.id);
    goNext();
  };

  return (
    <div className="registration-wizard">
      {/* Progress indicator */}
      <nav className="registration-wizard__progress" aria-label="Registration progress">
        <ol className="registration-wizard__steps">
          {STEPS.map((step, index) => (
            <li
              key={step.id}
              className={`registration-wizard__step ${
                index === currentStepIndex
                  ? 'registration-wizard__step--current'
                  : index < currentStepIndex
                    ? 'registration-wizard__step--completed'
                    : ''
              }`}
            >
              <button
                type="button"
                onClick={() => index < currentStepIndex && goToStep(step.id)}
                disabled={index > currentStepIndex}
                aria-current={index === currentStepIndex ? 'step' : undefined}
              >
                <span className="registration-wizard__step-number">{index + 1}</span>
                <span className="registration-wizard__step-label">{step.label}</span>
              </button>
            </li>
          ))}
        </ol>
      </nav>

      {/* Step content */}
      <Card className="registration-wizard__content">
        <CardBody>
          {currentStep === 'select' && (
            <div className="registration-wizard__section">
              <StoryPicker onSelect={handleStorySelect} selectedId={selectedStory?.id} />
            </div>
          )}

          {currentStep === 'contributors' && selectedStory && (
            <div className="registration-wizard__section">
              <h2>Manage Contributors</h2>
              <p>Add and verify all contributors for this story.</p>
              <ContributorManager storyId={selectedStory.id} />
            </div>
          )}

          {currentStep === 'splits' && selectedStory && (
            <div className="registration-wizard__section">
              <h2>Set Royalty Splits</h2>
              <p>Define how royalties will be distributed among contributors.</p>
              <RoyaltySplitEditor storyId={selectedStory.id} />
            </div>
          )}

          {currentStep === 'review' && selectedStory && (
            <div className="registration-wizard__section">
              <h2>Review Registration</h2>
              <p>Verify all details before submitting for on-chain registration.</p>
              <div className="registration-wizard__review">
                <h3>{selectedStory.title}</h3>
                <p>{selectedStory.summary}</p>
                <h4>Contributors</h4>
                <ul>
                  {selectedStory.contributors.map(c => (
                    <li key={c.userId}>
                      {c.userName} - {c.role} ({c.split}%)
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          )}

          {currentStep === 'register' && (
            <div className="registration-wizard__section">
              <h2>Registration Status</h2>
              <RegistrationStatus
                storyId={selectedStory?.id ?? ''}
                result={registrationResult}
                isLoading={isRegistering}
              />
            </div>
          )}
        </CardBody>

        <CardFooter>
          <div className="registration-wizard__actions">
            {currentStep !== 'select' && currentStep !== 'register' && (
              <Button variant="outline" onClick={goBack}>
                Back
              </Button>
            )}
            {currentStep === 'select' && (
              <Button onClick={goNext} disabled={!selectedStory}>
                Continue
              </Button>
            )}
            {currentStep === 'contributors' && <Button onClick={goNext}>Continue to Splits</Button>}
            {currentStep === 'splits' && <Button onClick={goNext}>Review</Button>}
            {currentStep === 'review' && (
              <Button onClick={handleRegister} loading={isRegistering}>
                Register On-Chain
              </Button>
            )}
          </div>
        </CardFooter>
      </Card>
    </div>
  );
}
