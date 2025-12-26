/**
 * Domain events for Mystira platform.
 * Mirrors Mystira.Shared.Messaging.Events in C#.
 */

/**
 * Base interface for all domain events.
 */
export interface DomainEvent {
  /** When the event occurred (ISO 8601) */
  occurredAt: string;
}

/**
 * Base interface for integration events (cross-service).
 */
export interface IntegrationEvent extends DomainEvent {
  /** Unique event ID for idempotency */
  eventId: string;
}

// =============================================================================
// Account Events
// =============================================================================

/**
 * Published when a new user account is created.
 */
export interface AccountCreated extends IntegrationEvent {
  type: 'AccountCreated';
  accountId: string;
  email: string;
  provider?: string;
}

/**
 * Published when a user account is updated.
 */
export interface AccountUpdated extends IntegrationEvent {
  type: 'AccountUpdated';
  accountId: string;
  updatedFields: string[];
}

/**
 * Published when a user account is deleted.
 */
export interface AccountDeleted extends IntegrationEvent {
  type: 'AccountDeleted';
  accountId: string;
  isSoftDelete: boolean;
}

// =============================================================================
// Session Events
// =============================================================================

/**
 * Published when a game session is started.
 */
export interface SessionStarted extends IntegrationEvent {
  type: 'SessionStarted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
}

/**
 * Published when a game session is completed.
 */
export interface SessionCompleted extends IntegrationEvent {
  type: 'SessionCompleted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
  durationSeconds: number;
  outcome?: string;
}

/**
 * Published when a session is abandoned.
 */
export interface SessionAbandoned extends IntegrationEvent {
  type: 'SessionAbandoned';
  sessionId: string;
  accountId: string;
  durationSeconds: number;
  lastProgressPoint?: string;
}

// =============================================================================
// Content Events
// =============================================================================

/**
 * Published when a scenario is created.
 */
export interface ScenarioCreated extends IntegrationEvent {
  type: 'ScenarioCreated';
  scenarioId: string;
  title: string;
  authorId: string;
  isPublished: boolean;
}

/**
 * Published when a scenario is updated.
 */
export interface ScenarioUpdated extends IntegrationEvent {
  type: 'ScenarioUpdated';
  scenarioId: string;
  updatedFields: string[];
  version?: number;
}

/**
 * Published when a scenario is published.
 */
export interface ScenarioPublished extends IntegrationEvent {
  type: 'ScenarioPublished';
  scenarioId: string;
  title: string;
  authorId: string;
}

/**
 * Published when a scenario is unpublished.
 */
export interface ScenarioUnpublished extends IntegrationEvent {
  type: 'ScenarioUnpublished';
  scenarioId: string;
  reason?: string;
}

/**
 * Published when media is uploaded.
 */
export interface MediaUploaded extends IntegrationEvent {
  type: 'MediaUploaded';
  mediaId: string;
  uploaderId: string;
  mimeType: string;
  sizeBytes: number;
}

// =============================================================================
// Cache Events
// =============================================================================

/**
 * Published when cache should be invalidated.
 */
export interface CacheInvalidated extends IntegrationEvent {
  type: 'CacheInvalidated';
  keyPattern: string;
  entityType: string;
  entityId?: string;
  sourceService: string;
}

/**
 * Published when cache warmup is requested.
 */
export interface CacheWarmupRequested extends IntegrationEvent {
  type: 'CacheWarmupRequested';
  entityType: string;
  entityIds?: string[];
  priority: number;
}

// =============================================================================
// AI Events
// =============================================================================

/**
 * Published when story generation is requested.
 */
export interface StoryGenerationRequested extends IntegrationEvent {
  type: 'StoryGenerationRequested';
  requestId: string;
  scenarioId: string;
  accountId: string;
  prompt?: string;
}

/**
 * Published when story generation completes successfully.
 */
export interface StoryGenerationCompleted extends IntegrationEvent {
  type: 'StoryGenerationCompleted';
  requestId: string;
  scenarioId: string;
  durationMs: number;
  tokensUsed?: number;
  model?: string;
}

/**
 * Published when story generation fails.
 */
export interface StoryGenerationFailed extends IntegrationEvent {
  type: 'StoryGenerationFailed';
  requestId: string;
  scenarioId: string;
  errorCode: string;
  errorMessage: string;
  isRetryable: boolean;
}

// =============================================================================
// User Events
// =============================================================================

/**
 * Published when a user logs in.
 */
export interface UserLoggedIn extends IntegrationEvent {
  type: 'UserLoggedIn';
  accountId: string;
  provider: string;
  clientIp?: string;
  userAgent?: string;
}

/**
 * Published when a user logs out.
 */
export interface UserLoggedOut extends IntegrationEvent {
  type: 'UserLoggedOut';
  accountId: string;
  isExplicit: boolean;
}

/**
 * Published when a password reset is requested.
 */
export interface PasswordResetRequested extends IntegrationEvent {
  type: 'PasswordResetRequested';
  accountId: string;
  email: string;
  expiresAt: string;
}

/**
 * Published when a password is successfully changed.
 */
export interface PasswordChanged extends IntegrationEvent {
  type: 'PasswordChanged';
  accountId: string;
  viaReset: boolean;
}

// =============================================================================
// Notification Events
// =============================================================================

/**
 * Published when a notification is sent.
 */
export interface NotificationSent extends IntegrationEvent {
  type: 'NotificationSent';
  notificationId: string;
  recipientId: string;
  notificationType: string;
  template: string;
}

/**
 * Published when an email is sent.
 */
export interface EmailSent extends IntegrationEvent {
  type: 'EmailSent';
  emailId: string;
  recipientEmail: string;
  template: string;
  subject: string;
}

/**
 * Published when a notification delivery fails.
 */
export interface NotificationFailed extends IntegrationEvent {
  type: 'NotificationFailed';
  notificationId: string;
  recipientId: string;
  reason: string;
  isRetryable: boolean;
}

// =============================================================================
// Progression Events
// =============================================================================

/**
 * Published when a user makes a choice in a story.
 */
export interface ChoiceMade extends IntegrationEvent {
  type: 'ChoiceMade';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  nodeId: string;
  choiceId: string;
  decisionTimeSeconds?: number;
}

/**
 * Published when a user starts a chapter.
 */
export interface ChapterStarted extends IntegrationEvent {
  type: 'ChapterStarted';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  chapterId: string;
  chapterNumber: number;
  isFirstPlay: boolean;
}

/**
 * Published when a user completes a chapter.
 */
export interface ChapterCompleted extends IntegrationEvent {
  type: 'ChapterCompleted';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  chapterId: string;
  chapterNumber: number;
  durationSeconds: number;
  choicesMade: number;
  outcome?: string;
}

/**
 * Published when a user reaches a checkpoint.
 */
export interface CheckpointReached extends IntegrationEvent {
  type: 'CheckpointReached';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  checkpointId: string;
  checkpointName?: string;
  progressPercent: number;
}

/**
 * Published when user takes a specific story branch.
 */
export interface StoryBranchTaken extends IntegrationEvent {
  type: 'StoryBranchTaken';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  branchId: string;
  branchName?: string;
  fromNodeId: string;
  toNodeId: string;
}

/**
 * Published when user explicitly saves their progress.
 */
export interface ProgressSaved extends IntegrationEvent {
  type: 'ProgressSaved';
  sessionId: string;
  scenarioId: string;
  accountId: string;
  saveSlotId: string;
  currentNodeId: string;
  progressPercent: number;
  isOverwrite: boolean;
}

// =============================================================================
// Gamification Events
// =============================================================================

/**
 * Published when a user unlocks an achievement.
 */
export interface AchievementUnlocked extends IntegrationEvent {
  type: 'AchievementUnlocked';
  accountId: string;
  achievementId: string;
  achievementName: string;
  category: string;
  rarity?: string;
  xpReward: number;
  scenarioId?: string;
}

/**
 * Published when a user earns a badge.
 */
export interface BadgeEarned extends IntegrationEvent {
  type: 'BadgeEarned';
  accountId: string;
  badgeId: string;
  badgeName: string;
  tier?: string;
  earnedVia?: string;
}

/**
 * Published when a user earns experience points.
 */
export interface XPEarned extends IntegrationEvent {
  type: 'XPEarned';
  accountId: string;
  amount: number;
  source: string;
  totalXP: number;
  multiplier: number;
  relatedEntityId?: string;
}

/**
 * Published when a user levels up.
 */
export interface LevelUp extends IntegrationEvent {
  type: 'LevelUp';
  accountId: string;
  fromLevel: number;
  toLevel: number;
  totalXP: number;
  unlockedRewards?: string[];
}

/**
 * Published when a user's streak is updated.
 */
export interface StreakUpdated extends IntegrationEvent {
  type: 'StreakUpdated';
  accountId: string;
  streakType: string;
  currentStreak: number;
  previousStreak: number;
  longestStreak: number;
  wasBroken: boolean;
  bonusXP: number;
}

/**
 * Published when leaderboard position changes.
 */
export interface LeaderboardUpdated extends IntegrationEvent {
  type: 'LeaderboardUpdated';
  accountId: string;
  leaderboardId: string;
  previousRank: number;
  newRank: number;
  score: number;
  period: string;
}

// =============================================================================
// Payment Events
// =============================================================================

/**
 * Published when a subscription is started.
 */
export interface SubscriptionStarted extends IntegrationEvent {
  type: 'SubscriptionStarted';
  accountId: string;
  subscriptionId: string;
  planId: string;
  planName: string;
  billingInterval: string;
  amountCents: number;
  currency: string;
  isTrial: boolean;
  trialEndsAt?: string;
}

/**
 * Published when a subscription is renewed.
 */
export interface SubscriptionRenewed extends IntegrationEvent {
  type: 'SubscriptionRenewed';
  accountId: string;
  subscriptionId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  nextBillingDate: string;
}

/**
 * Published when a subscription is cancelled.
 */
export interface SubscriptionCancelled extends IntegrationEvent {
  type: 'SubscriptionCancelled';
  accountId: string;
  subscriptionId: string;
  reason?: string;
  isImmediate: boolean;
  accessEndsAt: string;
  refundRequested: boolean;
}

/**
 * Published when a payment succeeds.
 */
export interface PaymentSucceeded extends IntegrationEvent {
  type: 'PaymentSucceeded';
  accountId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  paymentMethod: string;
  productType: string;
  productId: string;
  externalTransactionId?: string;
}

/**
 * Published when a payment fails.
 */
export interface PaymentFailed extends IntegrationEvent {
  type: 'PaymentFailed';
  accountId: string;
  paymentId: string;
  amountCents: number;
  currency: string;
  failureCode: string;
  failureMessage: string;
  isRetryable: boolean;
  retryAttempt: number;
}

/**
 * Published when a refund is processed.
 */
export interface RefundProcessed extends IntegrationEvent {
  type: 'RefundProcessed';
  accountId: string;
  refundId: string;
  originalPaymentId: string;
  amountCents: number;
  currency: string;
  reason: string;
  isPartial: boolean;
}

/**
 * Published when premium content is unlocked.
 */
export interface PremiumContentUnlocked extends IntegrationEvent {
  type: 'PremiumContentUnlocked';
  accountId: string;
  contentType: string;
  contentId: string;
  unlockMethod: string;
  paymentId?: string;
}

// =============================================================================
// Social Events
// =============================================================================

/**
 * Published when a user rates a scenario.
 */
export interface ScenarioRated extends IntegrationEvent {
  type: 'ScenarioRated';
  accountId: string;
  scenarioId: string;
  rating: number;
  previousRating?: number;
}

/**
 * Published when a user reviews a scenario.
 */
export interface ScenarioReviewed extends IntegrationEvent {
  type: 'ScenarioReviewed';
  reviewId: string;
  accountId: string;
  scenarioId: string;
  rating: number;
  hasTextContent: boolean;
  textLength: number;
  hasCompletedScenario: boolean;
}

/**
 * Published when a user shares a scenario.
 */
export interface ScenarioShared extends IntegrationEvent {
  type: 'ScenarioShared';
  accountId: string;
  scenarioId: string;
  platform: string;
  shareId: string;
}

/**
 * Published when a user follows another user.
 */
export interface UserFollowed extends IntegrationEvent {
  type: 'UserFollowed';
  followerAccountId: string;
  followedAccountId: string;
  isMutual: boolean;
}

/**
 * Published when a user unfollows another user.
 */
export interface UserUnfollowed extends IntegrationEvent {
  type: 'UserUnfollowed';
  followerAccountId: string;
  unfollowedAccountId: string;
}

/**
 * Published when a comment is posted.
 */
export interface CommentPosted extends IntegrationEvent {
  type: 'CommentPosted';
  commentId: string;
  accountId: string;
  targetType: string;
  targetId: string;
  parentCommentId?: string;
  textLength: number;
}

/**
 * Published when a comment is deleted.
 */
export interface CommentDeleted extends IntegrationEvent {
  type: 'CommentDeleted';
  commentId: string;
  accountId: string;
  deletedBy: string;
  reason?: string;
}

/**
 * Published when a user likes content.
 */
export interface ContentLiked extends IntegrationEvent {
  type: 'ContentLiked';
  accountId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
}

/**
 * Published when a user unlikes content.
 */
export interface ContentUnliked extends IntegrationEvent {
  type: 'ContentUnliked';
  accountId: string;
  contentType: string;
  contentId: string;
}

// =============================================================================
// Moderation Events
// =============================================================================

/**
 * Published when content is reported by a user.
 */
export interface ContentReported extends IntegrationEvent {
  type: 'ContentReported';
  reportId: string;
  reporterAccountId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
  category: string;
  hasDetails: boolean;
}

/**
 * Published when content is moderated.
 */
export interface ContentModerated extends IntegrationEvent {
  type: 'ContentModerated';
  actionId: string;
  contentType: string;
  contentId: string;
  contentOwnerId: string;
  moderatorId: string;
  action: string;
  reason: string;
  relatedReportIds?: string[];
}

/**
 * Published when a user receives a warning.
 */
export interface UserWarned extends IntegrationEvent {
  type: 'UserWarned';
  warningId: string;
  accountId: string;
  moderatorId: string;
  category: string;
  severity: string;
  warningCount: number;
  relatedContentId?: string;
}

/**
 * Published when a user is suspended.
 */
export interface UserSuspended extends IntegrationEvent {
  type: 'UserSuspended';
  suspensionId: string;
  accountId: string;
  moderatorId: string;
  reason: string;
  durationHours?: number;
  endsAt?: string;
  canAppeal: boolean;
}

/**
 * Published when a user is banned.
 */
export interface UserBanned extends IntegrationEvent {
  type: 'UserBanned';
  banId: string;
  accountId: string;
  moderatorId: string;
  reason: string;
  isPermanent: boolean;
  endsAt?: string;
  canAppeal: boolean;
}

/**
 * Published when a user submits an appeal.
 */
export interface AppealSubmitted extends IntegrationEvent {
  type: 'AppealSubmitted';
  appealId: string;
  accountId: string;
  appealType: string;
  relatedActionId: string;
}

/**
 * Published when an appeal is resolved.
 */
export interface AppealResolved extends IntegrationEvent {
  type: 'AppealResolved';
  appealId: string;
  accountId: string;
  moderatorId: string;
  outcome: string;
  notes?: string;
}

// =============================================================================
// Onboarding Events
// =============================================================================

/**
 * Published when a user's email is verified.
 */
export interface EmailVerified extends IntegrationEvent {
  type: 'EmailVerified';
  accountId: string;
  email: string;
  verificationMethod: string;
}

/**
 * Published when onboarding flow begins.
 */
export interface OnboardingStarted extends IntegrationEvent {
  type: 'OnboardingStarted';
  accountId: string;
  onboardingVersion: string;
  source?: string;
}

/**
 * Published when an onboarding step is completed.
 */
export interface OnboardingStepCompleted extends IntegrationEvent {
  type: 'OnboardingStepCompleted';
  accountId: string;
  stepId: string;
  stepName: string;
  stepNumber: number;
  totalSteps: number;
  skipped: boolean;
}

/**
 * Published when full onboarding is completed.
 */
export interface OnboardingCompleted extends IntegrationEvent {
  type: 'OnboardingCompleted';
  accountId: string;
  durationSeconds: number;
  stepsCompleted: number;
  stepsSkipped: number;
}

/**
 * Published when onboarding is entirely skipped.
 */
export interface OnboardingSkipped extends IntegrationEvent {
  type: 'OnboardingSkipped';
  accountId: string;
  atStep: number;
}

/**
 * Published when profile setup is completed.
 */
export interface ProfileCompleted extends IntegrationEvent {
  type: 'ProfileCompleted';
  accountId: string;
  displayNameSet: boolean;
  avatarSet: boolean;
  preferencesSet: boolean;
}

/**
 * Published when a user signs up via referral.
 */
export interface ReferralSignup extends IntegrationEvent {
  type: 'ReferralSignup';
  accountId: string;
  referrerAccountId: string;
  referralCode: string;
  campaign?: string;
}

/**
 * Published when referral reward is granted.
 */
export interface ReferralRewardGranted extends IntegrationEvent {
  type: 'ReferralRewardGranted';
  accountId: string;
  referredAccountId: string;
  rewardType: string;
  rewardValue: string;
}

// =============================================================================
// Bundle Events
// =============================================================================

/**
 * Published when a content bundle is created.
 */
export interface BundleCreated extends IntegrationEvent {
  type: 'BundleCreated';
  bundleId: string;
  bundleName: string;
  creatorId: string;
  itemCount: number;
  priceCents?: number;
  currency?: string;
}

/**
 * Published when a bundle is purchased.
 */
export interface BundlePurchased extends IntegrationEvent {
  type: 'BundlePurchased';
  accountId: string;
  bundleId: string;
  paymentId: string;
  priceCents: number;
  currency: string;
  discountApplied: boolean;
}

/**
 * Published when a bundle is activated.
 */
export interface BundleActivated extends IntegrationEvent {
  type: 'BundleActivated';
  accountId: string;
  bundleId: string;
  itemsUnlocked: string[];
}

/**
 * Published when a bundle is gifted.
 */
export interface BundleGifted extends IntegrationEvent {
  type: 'BundleGifted';
  senderAccountId: string;
  recipientAccountId: string;
  bundleId: string;
  paymentId: string;
  giftMessage?: string;
}

/**
 * Published when a promo code is redeemed.
 */
export interface PromoCodeRedeemed extends IntegrationEvent {
  type: 'PromoCodeRedeemed';
  accountId: string;
  promoCode: string;
  discountType: string;
  discountValue: number;
  appliedTo?: string;
}

// =============================================================================
// Co-op/Partner Events
// =============================================================================

/**
 * Published when a partner invite is sent.
 */
export interface PartnerInviteSent extends IntegrationEvent {
  type: 'PartnerInviteSent';
  inviteId: string;
  senderAccountId: string;
  recipientAccountId: string;
  scenarioId?: string;
  expiresAt: string;
}

/**
 * Published when a partner invite is accepted.
 */
export interface PartnerInviteAccepted extends IntegrationEvent {
  type: 'PartnerInviteAccepted';
  inviteId: string;
  senderAccountId: string;
  recipientAccountId: string;
  scenarioId?: string;
}

/**
 * Published when a partner invite is declined.
 */
export interface PartnerInviteDeclined extends IntegrationEvent {
  type: 'PartnerInviteDeclined';
  inviteId: string;
  senderAccountId: string;
  recipientAccountId: string;
  reason?: string;
}

/**
 * Published when a co-op session is started.
 */
export interface CoopSessionStarted extends IntegrationEvent {
  type: 'CoopSessionStarted';
  coopSessionId: string;
  hostAccountId: string;
  scenarioId: string;
  maxPartners: number;
}

/**
 * Published when a partner joins a session.
 */
export interface PartnerJoinedSession extends IntegrationEvent {
  type: 'PartnerJoinedSession';
  coopSessionId: string;
  partnerAccountId: string;
  joinMethod: string;
  currentPartnerCount: number;
}

/**
 * Published when a partner leaves a session.
 */
export interface PartnerLeftSession extends IntegrationEvent {
  type: 'PartnerLeftSession';
  coopSessionId: string;
  partnerAccountId: string;
  reason: string;
  remainingPartners: number;
}

/**
 * Published when a co-op session ends.
 */
export interface CoopSessionEnded extends IntegrationEvent {
  type: 'CoopSessionEnded';
  coopSessionId: string;
  hostAccountId: string;
  durationSeconds: number;
  participantCount: number;
  endReason: string;
}

/**
 * Published when a turn is passed in co-op.
 */
export interface TurnPassed extends IntegrationEvent {
  type: 'TurnPassed';
  coopSessionId: string;
  fromAccountId: string;
  toAccountId: string;
  turnNumber: number;
}

// =============================================================================
// Chat/Messaging Events
// =============================================================================

/**
 * Published when a conversation is started.
 */
export interface ConversationStarted extends IntegrationEvent {
  type: 'ConversationStarted';
  conversationId: string;
  initiatorAccountId: string;
  participantIds: string[];
  conversationType: string;
  contextId?: string;
}

/**
 * Published when a message is sent.
 */
export interface MessageSent extends IntegrationEvent {
  type: 'MessageSent';
  messageId: string;
  conversationId: string;
  senderAccountId: string;
  messageType: string;
  contentLength: number;
  hasAttachments: boolean;
}

/**
 * Published when a message is delivered.
 */
export interface MessageDelivered extends IntegrationEvent {
  type: 'MessageDelivered';
  messageId: string;
  conversationId: string;
  recipientAccountId: string;
  deliveryLatencyMs: number;
}

/**
 * Published when a message is read.
 */
export interface MessageRead extends IntegrationEvent {
  type: 'MessageRead';
  messageId: string;
  conversationId: string;
  readerAccountId: string;
  readDelaySeconds: number;
}

/**
 * Published when a message is deleted.
 */
export interface MessageDeleted extends IntegrationEvent {
  type: 'MessageDeleted';
  messageId: string;
  conversationId: string;
  deletedByAccountId: string;
  deletedForEveryone: boolean;
}

/**
 * Published when user starts typing.
 */
export interface TypingStarted extends IntegrationEvent {
  type: 'TypingStarted';
  conversationId: string;
  accountId: string;
}

/**
 * Published when a conversation is archived.
 */
export interface ConversationArchived extends IntegrationEvent {
  type: 'ConversationArchived';
  conversationId: string;
  archivedByAccountId: string;
}

/**
 * Published when a user is muted in a conversation.
 */
export interface UserMutedInConversation extends IntegrationEvent {
  type: 'UserMutedInConversation';
  conversationId: string;
  mutedAccountId: string;
  mutedByAccountId: string;
  durationMinutes?: number;
}

// =============================================================================
// Device/Sync Events
// =============================================================================

/**
 * Published when a device is registered.
 */
export interface DeviceRegistered extends IntegrationEvent {
  type: 'DeviceRegistered';
  accountId: string;
  deviceId: string;
  deviceType: string;
  deviceName: string;
  osVersion?: string;
  appVersion: string;
}

/**
 * Published when a device is removed.
 */
export interface DeviceRemoved extends IntegrationEvent {
  type: 'DeviceRemoved';
  accountId: string;
  deviceId: string;
  removedBy: string;
  reason: string;
}

/**
 * Published when a session is synced across devices.
 */
export interface SessionSynced extends IntegrationEvent {
  type: 'SessionSynced';
  accountId: string;
  sessionId: string;
  fromDeviceId: string;
  toDeviceId: string;
  dataSize: number;
}

/**
 * Published when a sync conflict is detected.
 */
export interface SyncConflictDetected extends IntegrationEvent {
  type: 'SyncConflictDetected';
  accountId: string;
  sessionId: string;
  device1Id: string;
  device2Id: string;
  conflictType: string;
  resolution: string;
}

/**
 * Published when device handoff is initiated.
 */
export interface DeviceHandoffStarted extends IntegrationEvent {
  type: 'DeviceHandoffStarted';
  accountId: string;
  sessionId: string;
  fromDeviceId: string;
  toDeviceId: string;
}

/**
 * Published when device handoff is completed.
 */
export interface DeviceHandoffCompleted extends IntegrationEvent {
  type: 'DeviceHandoffCompleted';
  accountId: string;
  sessionId: string;
  fromDeviceId: string;
  toDeviceId: string;
  handoffDurationMs: number;
  success: boolean;
}

/**
 * Published when push notification settings are updated.
 */
export interface PushSettingsUpdated extends IntegrationEvent {
  type: 'PushSettingsUpdated';
  accountId: string;
  deviceId: string;
  pushEnabled: boolean;
  categories?: string[];
}

// =============================================================================
// Discovery/Recommendation Events
// =============================================================================

/**
 * Published when a search is performed.
 */
export interface SearchPerformed extends IntegrationEvent {
  type: 'SearchPerformed';
  accountId?: string;
  searchId: string;
  query: string;
  filters?: Record<string, string>;
  resultCount: number;
  searchType: string;
}

/**
 * Published when a search result is clicked.
 */
export interface SearchResultClicked extends IntegrationEvent {
  type: 'SearchResultClicked';
  accountId: string;
  searchId: string;
  resultId: string;
  resultType: string;
  position: number;
}

/**
 * Published when a recommendation is shown.
 */
export interface RecommendationShown extends IntegrationEvent {
  type: 'RecommendationShown';
  accountId: string;
  recommendationId: string;
  itemIds: string[];
  algorithm: string;
  context: string;
}

/**
 * Published when a recommendation is clicked.
 */
export interface RecommendationClicked extends IntegrationEvent {
  type: 'RecommendationClicked';
  accountId: string;
  recommendationId: string;
  itemId: string;
  position: number;
}

/**
 * Published when a recommendation is dismissed.
 */
export interface RecommendationDismissed extends IntegrationEvent {
  type: 'RecommendationDismissed';
  accountId: string;
  recommendationId: string;
  itemId?: string;
  reason?: string;
}

/**
 * Published when content is featured.
 */
export interface ContentFeatured extends IntegrationEvent {
  type: 'ContentFeatured';
  contentId: string;
  contentType: string;
  featuredBy: string;
  featuredUntil?: string;
  placement: string;
}

/**
 * Published when a tag is followed.
 */
export interface TagFollowed extends IntegrationEvent {
  type: 'TagFollowed';
  accountId: string;
  tagId: string;
  tagName: string;
}

/**
 * Published when a tag is unfollowed.
 */
export interface TagUnfollowed extends IntegrationEvent {
  type: 'TagUnfollowed';
  accountId: string;
  tagId: string;
  tagName: string;
}

// =============================================================================
// Inventory Events
// =============================================================================

/**
 * Published when an item is acquired.
 */
export interface ItemAcquired extends IntegrationEvent {
  type: 'ItemAcquired';
  accountId: string;
  itemId: string;
  itemType: string;
  itemName: string;
  quantity: number;
  acquisitionMethod: string;
  sourceId?: string;
}

/**
 * Published when an item is used/consumed.
 */
export interface ItemUsed extends IntegrationEvent {
  type: 'ItemUsed';
  accountId: string;
  itemId: string;
  quantity: number;
  contextId?: string;
  remainingQuantity: number;
}

/**
 * Published when a cosmetic item is equipped.
 */
export interface ItemEquipped extends IntegrationEvent {
  type: 'ItemEquipped';
  accountId: string;
  itemId: string;
  slot: string;
  previousItemId?: string;
}

/**
 * Published when an item is unequipped.
 */
export interface ItemUnequipped extends IntegrationEvent {
  type: 'ItemUnequipped';
  accountId: string;
  itemId: string;
  slot: string;
}

/**
 * Published when virtual currency is earned.
 */
export interface CurrencyEarned extends IntegrationEvent {
  type: 'CurrencyEarned';
  accountId: string;
  currencyType: string;
  amount: number;
  source: string;
  newBalance: number;
}

/**
 * Published when virtual currency is spent.
 */
export interface CurrencySpent extends IntegrationEvent {
  type: 'CurrencySpent';
  accountId: string;
  currencyType: string;
  amount: number;
  spentOn: string;
  itemId?: string;
  newBalance: number;
}

/**
 * Published when items are traded between users.
 */
export interface ItemTraded extends IntegrationEvent {
  type: 'ItemTraded';
  tradeId: string;
  user1AccountId: string;
  user2AccountId: string;
  user1ItemIds: string[];
  user2ItemIds: string[];
}

/**
 * Published when daily/login reward is claimed.
 */
export interface DailyRewardClaimed extends IntegrationEvent {
  type: 'DailyRewardClaimed';
  accountId: string;
  streakDay: number;
  rewardType: string;
  rewardValue: string;
  streakBonusApplied: boolean;
}

// =============================================================================
// Preference Events
// =============================================================================

/**
 * Published when user preferences are updated.
 */
export interface PreferencesUpdated extends IntegrationEvent {
  type: 'PreferencesUpdated';
  accountId: string;
  category: string;
  key: string;
  newValue: string;
  previousValue?: string;
}

/**
 * Published when theme is changed.
 */
export interface ThemeChanged extends IntegrationEvent {
  type: 'ThemeChanged';
  accountId: string;
  fromTheme: string;
  toTheme: string;
}

/**
 * Published when language is changed.
 */
export interface LanguageChanged extends IntegrationEvent {
  type: 'LanguageChanged';
  accountId: string;
  fromLanguage: string;
  toLanguage: string;
}

/**
 * Published when accessibility settings change.
 */
export interface AccessibilitySettingChanged extends IntegrationEvent {
  type: 'AccessibilitySettingChanged';
  accountId: string;
  setting: string;
  enabled: boolean;
  value?: string;
}

/**
 * Published when privacy settings change.
 */
export interface PrivacySettingChanged extends IntegrationEvent {
  type: 'PrivacySettingChanged';
  accountId: string;
  setting: string;
  newValue: string;
}

/**
 * Published when notification preferences change.
 */
export interface NotificationPreferenceChanged extends IntegrationEvent {
  type: 'NotificationPreferenceChanged';
  accountId: string;
  channel: string;
  category: string;
  enabled: boolean;
}

/**
 * Published when content preferences change.
 */
export interface ContentPreferenceUpdated extends IntegrationEvent {
  type: 'ContentPreferenceUpdated';
  accountId: string;
  preferenceType: string;
  values: string[];
}

/**
 * Published when parental controls are updated.
 */
export interface ParentalControlsUpdated extends IntegrationEvent {
  type: 'ParentalControlsUpdated';
  parentAccountId: string;
  childAccountId?: string;
  controlType: string;
  restrictionValue: string;
}

// =============================================================================
// Season/Battle Pass Events
// =============================================================================

/**
 * Published when a new season starts.
 */
export interface SeasonStarted extends IntegrationEvent {
  type: 'SeasonStarted';
  seasonId: string;
  seasonName: string;
  startDate: string;
  endDate: string;
  seasonNumber?: number;
}

/**
 * Published when a season ends.
 */
export interface SeasonEnded extends IntegrationEvent {
  type: 'SeasonEnded';
  seasonId: string;
  totalParticipants?: number;
}

/**
 * Published when a user purchases a battle pass.
 */
export interface BattlePassPurchased extends IntegrationEvent {
  type: 'BattlePassPurchased';
  accountId: string;
  seasonId: string;
  passType: string;
  purchasePrice?: number;
}

/**
 * Published when a user completes a battle pass tier.
 */
export interface BattlePassTierCompleted extends IntegrationEvent {
  type: 'BattlePassTierCompleted';
  accountId: string;
  seasonId: string;
  tier: number;
  rewardsUnlocked: string[];
}

/**
 * Published when a seasonal reward is claimed.
 */
export interface SeasonalRewardClaimed extends IntegrationEvent {
  type: 'SeasonalRewardClaimed';
  accountId: string;
  seasonId: string;
  rewardId: string;
  rewardType: string;
  tier?: number;
}

// =============================================================================
// Tutorial Events
// =============================================================================

/**
 * Published when a tutorial is started.
 */
export interface TutorialStarted extends IntegrationEvent {
  type: 'TutorialStarted';
  accountId: string;
  tutorialId: string;
  tutorialName?: string;
}

/**
 * Published when a tutorial step is completed.
 */
export interface TutorialStepCompleted extends IntegrationEvent {
  type: 'TutorialStepCompleted';
  accountId: string;
  tutorialId: string;
  stepId: string;
  stepNumber: number;
  durationSeconds?: number;
}

/**
 * Published when a tutorial is fully completed.
 */
export interface TutorialCompleted extends IntegrationEvent {
  type: 'TutorialCompleted';
  accountId: string;
  tutorialId: string;
  totalDurationSeconds: number;
  stepsCompleted: number;
}

/**
 * Published when a tutorial is skipped.
 */
export interface TutorialSkipped extends IntegrationEvent {
  type: 'TutorialSkipped';
  accountId: string;
  tutorialId: string;
  lastCompletedStep?: string;
  reason?: string;
}

// =============================================================================
// Matchmaking Events
// =============================================================================

/**
 * Published when matchmaking queue is entered.
 */
export interface MatchmakingStarted extends IntegrationEvent {
  type: 'MatchmakingStarted';
  accountId: string;
  queueId: string;
  gameMode: string;
  preferences?: Record<string, any>;
}

/**
 * Published when a match is found.
 */
export interface MatchFound extends IntegrationEvent {
  type: 'MatchFound';
  matchId: string;
  participants: string[];
  waitTimeSeconds: number;
  averageSkillLevel?: number;
}

/**
 * Published when a player accepts a match.
 */
export interface MatchAccepted extends IntegrationEvent {
  type: 'MatchAccepted';
  accountId: string;
  matchId: string;
}

/**
 * Published when a player declines a match.
 */
export interface MatchDeclined extends IntegrationEvent {
  type: 'MatchDeclined';
  accountId: string;
  matchId: string;
  reason?: string;
}

/**
 * Published when matchmaking is cancelled.
 */
export interface MatchmakingCancelled extends IntegrationEvent {
  type: 'MatchmakingCancelled';
  accountId: string;
  queueId: string;
  timeInQueueSeconds: number;
}

// =============================================================================
// Voice/Audio Events
// =============================================================================

/**
 * Published when a user joins voice chat.
 */
export interface VoiceChatJoined extends IntegrationEvent {
  type: 'VoiceChatJoined';
  accountId: string;
  channelId: string;
  sessionId?: string;
}

/**
 * Published when a user leaves voice chat.
 */
export interface VoiceChatLeft extends IntegrationEvent {
  type: 'VoiceChatLeft';
  accountId: string;
  channelId: string;
  durationSeconds: number;
}

/**
 * Published when voice chat is muted.
 */
export interface VoiceChatMuted extends IntegrationEvent {
  type: 'VoiceChatMuted';
  accountId: string;
  channelId: string;
  mutedByAccountId?: string;
}

/**
 * Published when voice chat is unmuted.
 */
export interface VoiceChatUnmuted extends IntegrationEvent {
  type: 'VoiceChatUnmuted';
  accountId: string;
  channelId: string;
}

/**
 * Published when audio settings are changed.
 */
export interface AudioSettingsChanged extends IntegrationEvent {
  type: 'AudioSettingsChanged';
  accountId: string;
  setting: string;
  newValue: string;
}

// =============================================================================
// Localization Events
// =============================================================================

/**
 * Published when content translation is requested.
 */
export interface ContentTranslationRequested extends IntegrationEvent {
  type: 'ContentTranslationRequested';
  contentId: string;
  contentType: string;
  sourceLanguage: string;
  targetLanguage: string;
  requestedBy?: string;
}

/**
 * Published when content translation is completed.
 */
export interface ContentTranslationCompleted extends IntegrationEvent {
  type: 'ContentTranslationCompleted';
  contentId: string;
  targetLanguage: string;
  translationQuality?: string;
}

/**
 * Published when a localization string is updated.
 */
export interface LocalizationStringUpdated extends IntegrationEvent {
  type: 'LocalizationStringUpdated';
  stringKey: string;
  language: string;
  oldValue?: string;
  newValue: string;
}

/**
 * Published when translation feedback is submitted.
 */
export interface TranslationFeedbackSubmitted extends IntegrationEvent {
  type: 'TranslationFeedbackSubmitted';
  accountId: string;
  contentId: string;
  language: string;
  feedbackType: string;
  comment?: string;
}

// =============================================================================
// AI Quality Feedback Events
// =============================================================================

/**
 * Published when AI-generated content is rated.
 */
export interface AIContentRated extends IntegrationEvent {
  type: 'AIContentRated';
  accountId: string;
  contentId: string;
  rating: number;
  ratingCategory?: string;
}

/**
 * Published when feedback on AI generation is submitted.
 */
export interface AIGenerationFeedbackSubmitted extends IntegrationEvent {
  type: 'AIGenerationFeedbackSubmitted';
  accountId: string;
  generationId: string;
  feedbackType: string;
  comment?: string;
  tags?: string[];
}

/**
 * Published when AI response regeneration is requested.
 */
export interface AIResponseRegenerationRequested extends IntegrationEvent {
  type: 'AIResponseRegenerationRequested';
  accountId: string;
  originalGenerationId: string;
  reason?: string;
}

/**
 * Published when AI output is accepted by user.
 */
export interface AIOutputAccepted extends IntegrationEvent {
  type: 'AIOutputAccepted';
  accountId: string;
  generationId: string;
  editsMade: boolean;
}

/**
 * Published when AI hallucination is reported.
 */
export interface AIHallucinationReported extends IntegrationEvent {
  type: 'AIHallucinationReported';
  accountId: string;
  generationId: string;
  hallucinationType?: string;
  excerpt?: string;
}

// =============================================================================
// Union Types
// =============================================================================

/**
 * All account-related events.
 */
export type AccountEvent = AccountCreated | AccountUpdated | AccountDeleted;

/**
 * All session-related events.
 */
export type SessionEvent = SessionStarted | SessionCompleted | SessionAbandoned;

/**
 * All content-related events.
 */
export type ContentEvent =
  | ScenarioCreated
  | ScenarioUpdated
  | ScenarioPublished
  | ScenarioUnpublished
  | MediaUploaded;

/**
 * All cache-related events.
 */
export type CacheEvent = CacheInvalidated | CacheWarmupRequested;

/**
 * All AI-related events.
 */
export type AIEvent = StoryGenerationRequested | StoryGenerationCompleted | StoryGenerationFailed;

/**
 * All user authentication events.
 */
export type UserEvent = UserLoggedIn | UserLoggedOut | PasswordResetRequested | PasswordChanged;

/**
 * All notification-related events.
 */
export type NotificationEvent = NotificationSent | EmailSent | NotificationFailed;

/**
 * All progression-related events.
 */
export type ProgressionEvent =
  | ChoiceMade
  | ChapterStarted
  | ChapterCompleted
  | CheckpointReached
  | StoryBranchTaken
  | ProgressSaved;

/**
 * All gamification-related events.
 */
export type GamificationEvent =
  | AchievementUnlocked
  | BadgeEarned
  | XPEarned
  | LevelUp
  | StreakUpdated
  | LeaderboardUpdated;

/**
 * All payment-related events.
 */
export type PaymentEvent =
  | SubscriptionStarted
  | SubscriptionRenewed
  | SubscriptionCancelled
  | PaymentSucceeded
  | PaymentFailed
  | RefundProcessed
  | PremiumContentUnlocked;

/**
 * All social-related events.
 */
export type SocialEvent =
  | ScenarioRated
  | ScenarioReviewed
  | ScenarioShared
  | UserFollowed
  | UserUnfollowed
  | CommentPosted
  | CommentDeleted
  | ContentLiked
  | ContentUnliked;

/**
 * All moderation-related events.
 */
export type ModerationEvent =
  | ContentReported
  | ContentModerated
  | UserWarned
  | UserSuspended
  | UserBanned
  | AppealSubmitted
  | AppealResolved;

/**
 * All onboarding-related events.
 */
export type OnboardingEvent =
  | EmailVerified
  | OnboardingStarted
  | OnboardingStepCompleted
  | OnboardingCompleted
  | OnboardingSkipped
  | ProfileCompleted
  | ReferralSignup
  | ReferralRewardGranted;

/**
 * All bundle-related events.
 */
export type BundleEvent =
  | BundleCreated
  | BundlePurchased
  | BundleActivated
  | BundleGifted
  | PromoCodeRedeemed;

/**
 * All co-op/partner-related events.
 */
export type CoopEvent =
  | PartnerInviteSent
  | PartnerInviteAccepted
  | PartnerInviteDeclined
  | CoopSessionStarted
  | PartnerJoinedSession
  | PartnerLeftSession
  | CoopSessionEnded
  | TurnPassed;

/**
 * All chat/messaging-related events.
 */
export type ChatEvent =
  | ConversationStarted
  | MessageSent
  | MessageDelivered
  | MessageRead
  | MessageDeleted
  | TypingStarted
  | ConversationArchived
  | UserMutedInConversation;

/**
 * All device/sync-related events.
 */
export type DeviceEvent =
  | DeviceRegistered
  | DeviceRemoved
  | SessionSynced
  | SyncConflictDetected
  | DeviceHandoffStarted
  | DeviceHandoffCompleted
  | PushSettingsUpdated;

/**
 * All discovery/recommendation-related events.
 */
export type DiscoveryEvent =
  | SearchPerformed
  | SearchResultClicked
  | RecommendationShown
  | RecommendationClicked
  | RecommendationDismissed
  | ContentFeatured
  | TagFollowed
  | TagUnfollowed;

/**
 * All inventory-related events.
 */
export type InventoryEvent =
  | ItemAcquired
  | ItemUsed
  | ItemEquipped
  | ItemUnequipped
  | CurrencyEarned
  | CurrencySpent
  | ItemTraded
  | DailyRewardClaimed;

/**
 * All preference-related events.
 */
export type PreferenceEvent =
  | PreferencesUpdated
  | ThemeChanged
  | LanguageChanged
  | AccessibilitySettingChanged
  | PrivacySettingChanged
  | NotificationPreferenceChanged
  | ContentPreferenceUpdated
  | ParentalControlsUpdated;

/**
 * All season/battle pass-related events.
 */
export type SeasonEvent =
  | SeasonStarted
  | SeasonEnded
  | BattlePassPurchased
  | BattlePassTierCompleted
  | SeasonalRewardClaimed;

/**
 * All tutorial-related events.
 */
export type TutorialEvent =
  | TutorialStarted
  | TutorialStepCompleted
  | TutorialCompleted
  | TutorialSkipped;

/**
 * All matchmaking-related events.
 */
export type MatchmakingEvent =
  | MatchmakingStarted
  | MatchFound
  | MatchAccepted
  | MatchDeclined
  | MatchmakingCancelled;

/**
 * All voice/audio-related events.
 */
export type VoiceAudioEvent =
  | VoiceChatJoined
  | VoiceChatLeft
  | VoiceChatMuted
  | VoiceChatUnmuted
  | AudioSettingsChanged;

/**
 * All localization-related events.
 */
export type LocalizationEvent =
  | ContentTranslationRequested
  | ContentTranslationCompleted
  | LocalizationStringUpdated
  | TranslationFeedbackSubmitted;

/**
 * All AI quality feedback-related events.
 */
export type AIFeedbackEvent =
  | AIContentRated
  | AIGenerationFeedbackSubmitted
  | AIResponseRegenerationRequested
  | AIOutputAccepted
  | AIHallucinationReported;

/**
 * All Mystira domain events (149 total across 26 categories).
 */
export type MystiraEvent =
  | AccountEvent
  | SessionEvent
  | ContentEvent
  | CacheEvent
  | AIEvent
  | UserEvent
  | NotificationEvent
  | ProgressionEvent
  | GamificationEvent
  | PaymentEvent
  | SocialEvent
  | ModerationEvent
  | OnboardingEvent
  | BundleEvent
  | CoopEvent
  | ChatEvent
  | DeviceEvent
  | DiscoveryEvent
  | InventoryEvent
  | PreferenceEvent
  | SeasonEvent
  | TutorialEvent
  | MatchmakingEvent
  | VoiceAudioEvent
  | LocalizationEvent
  | AIFeedbackEvent;

/**
 * Event type discriminator values.
 */
export type EventType = MystiraEvent['type'];
