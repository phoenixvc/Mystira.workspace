describe('Story Registration Flow', () => {
  beforeEach(() => {
    cy.visit('/login');
    cy.get('input[type="email"]').type('alice@example.com');
    cy.get('input[type="password"]').type('password123');
    cy.get('button[type="submit"]').click();
    cy.url().should('include', '/dashboard');
  });

  it('should navigate to registration wizard', () => {
    cy.contains('Register New Story').click();
    cy.url().should('include', '/register');
    cy.contains('Select a Story to Register').should('be.visible');
  });

  it('should display story picker with filtering', () => {
    cy.visit('/register');
    cy.get('.story-picker').should('be.visible');
    cy.get('.story-picker__search').type('Crystal');
    cy.contains('The Crystal Kingdom').should('be.visible');
  });

  it('should show contributors step after selecting story', () => {
    cy.visit('/register');
    cy.contains('The Crystal Kingdom').click();
    cy.contains('Continue').click();
    cy.contains('Manage Contributors').should('be.visible');
  });

  it('should validate royalty splits total 100%', () => {
    cy.visit('/register');
    cy.contains('Garden of Dreams').click();
    cy.contains('Continue').click();
    cy.contains('Continue to Splits').click();
    cy.contains('Royalty Distribution').should('be.visible');
    cy.get('.royalty-split-editor__total-value').should('contain', '100%');
  });

  it('should show review step before registration', () => {
    cy.visit('/register');
    cy.contains('The Crystal Kingdom').click();
    cy.contains('Continue').click();
    cy.contains('Continue to Splits').click();
    cy.contains('Review').click();
    cy.contains('Review Registration').should('be.visible');
  });
});

describe('Accessibility', () => {
  it('should have no accessibility violations on login page', () => {
    cy.visit('/login');
    cy.injectAxe();
    cy.checkA11y();
  });

  it('should have no accessibility violations on dashboard', () => {
    cy.visit('/login');
    cy.get('input[type="email"]').type('alice@example.com');
    cy.get('input[type="password"]').type('password123');
    cy.get('button[type="submit"]').click();
    cy.injectAxe();
    cy.checkA11y();
  });
});
