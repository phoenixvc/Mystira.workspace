/**
 * App API Types
 *
 * TypeScript type definitions matching the C# contracts in Mystira.Contracts.App.
 * These types are auto-synced from the .NET contracts package.
 *
 * @see packages/contracts/dotnet/Mystira.Contracts/App/
 */

// =============================================================================
// Enums
// =============================================================================

/**
 * Represents the difficulty level of a scenario.
 */
export enum DifficultyLevel {
  Easy = 0,
  Medium = 1,
  Hard = 2,
  Expert = 3,
}

/**
 * Represents the expected duration of a game session.
 */
export enum SessionLength {
  /** Quick session, typically 15-30 minutes */
  Quick = 0,
  /** Short session, typically 30-60 minutes */
  Short = 1,
  /** Medium session, typically 1-2 hours */
  Medium = 2,
  /** Long session, typically 2-4 hours */
  Long = 3,
  /** Extended session, typically 4+ hours */
  Extended = 4,
}

/**
 * Represents the current state of a scenario game.
 */
export enum ScenarioGameState {
  NotStarted = 0,
  InProgress = 1,
  Paused = 2,
  Completed = 3,
  Abandoned = 4,
}

/**
 * Represents the subscription type for an account.
 */
export enum SubscriptionType {
  Free = 0,
  Basic = 1,
  Premium = 2,
  Family = 3,
  Enterprise = 4,
}

// =============================================================================
// Base Types
// =============================================================================

/**
 * Base request interface for API calls
 */
export interface ApiRequest {
  /** Request correlation ID for tracing */
  correlationId?: string;
}

/**
 * Base response interface for API calls
 */
export interface ApiResponse<T = unknown> {
  /** Whether the request was successful */
  success: boolean;
  /** Response data */
  data?: T;
  /** Error details if not successful */
  error?: ApiError;
}

/**
 * API error details
 */
export interface ApiError {
  /** Error code */
  code: string;
  /** Human-readable error message */
  message: string;
  /** Additional error details */
  details?: Record<string, unknown>;
}

// =============================================================================
// Scenario Request Types
// =============================================================================

/**
 * Character metadata request model
 */
export interface CharacterMetadataRequest {
  /** The roles of the character in the story */
  role?: string[];
  /** The archetype classifications of the character */
  archetype?: string[];
  /** The species of the character */
  species?: string;
  /** The age of the character */
  age?: number;
  /** List of character traits */
  traits?: string[];
  /** The backstory of the character */
  backstory?: string;
}

/**
 * Media references request model
 */
export interface MediaReferencesRequest {
  /** Image URL or identifier */
  image?: string;
  /** Audio URL or identifier */
  audio?: string;
  /** Video URL or identifier */
  video?: string;
}

/**
 * Branch (choice) in a scene
 */
export interface BranchRequest {
  /** The text displayed for this branch/choice */
  text?: string;
  /** The identifier of the scene this branch leads to */
  nextSceneId?: string;
  /** The compass axis affected by this choice */
  compassAxis?: string;
  /** The direction on the compass axis (positive/negative) */
  compassDirection?: string;
  /** The delta value for the compass axis */
  compassDelta?: number;
}

/**
 * Echo reveal condition
 */
export interface EchoRevealRequest {
  /** The condition that triggers the echo reveal */
  condition?: string;
  /** The message to display when revealed */
  message?: string;
  /** The tone of the echo reveal */
  tone?: string;
}

/**
 * Character definition in a scenario
 */
export interface CharacterRequest {
  /** Unique identifier for the character */
  id: string;
  /** The name of the character */
  name: string;
  /** A description of the character */
  description?: string;
  /** The role of the character in the story */
  role?: string;
  /** The archetype classification of the character */
  archetype?: string;
  /** Optional URL or identifier for the character's image */
  image?: string;
  /** Optional URL or identifier for the character's audio */
  audio?: string;
  /** Character metadata including role, archetype, species, etc. */
  metadata?: CharacterMetadataRequest;
  /** Optional list of traits associated with the character */
  traits?: string[];
  /** Whether this character is a player character */
  isPlayerCharacter?: boolean;
}

/**
 * Choice within a scene
 */
export interface ChoiceRequest {
  /** Optional unique identifier for the choice */
  id?: string;
  /** The text displayed for this choice */
  text: string;
  /** Optional identifier of the scene this choice leads to */
  nextSceneId?: string;
  /** Optional compass axis impacts for this choice */
  compassImpacts?: Record<string, number>;
  /** Additional metadata for the choice */
  metadata?: Record<string, unknown>;
}

/**
 * Scene definition in a scenario
 */
export interface SceneRequest {
  /** Unique identifier for the scene */
  id: string;
  /** The title of the scene */
  title: string;
  /** The type of scene (narrative, choice, or ending) */
  type: string;
  /** A description of the scene */
  description: string;
  /** The narrative content of the scene */
  content?: string;
  /** The order of this scene in the scenario */
  order: number;
  /** The identifier of the next scene (for linear progression) */
  nextSceneId?: string;
  /** The difficulty level of this scene */
  difficulty?: string;
  /** The active character in this scene */
  activeCharacter?: string;
  /** Media references for this scene */
  media?: MediaReferencesRequest;
  /** Optional URL or identifier for the scene background image */
  backgroundImage?: string;
  /** Optional URL or identifier for the scene background music */
  backgroundMusic?: string;
  /** Optional list of choices available in this scene */
  choices?: ChoiceRequest[];
  /** Optional list of branches (choices) in this scene */
  branches?: BranchRequest[];
  /** Optional list of echo reveals in this scene */
  echoReveals?: EchoRevealRequest[];
}

/**
 * Request to create a new scenario
 */
export interface CreateScenarioRequest {
  /** The title of the scenario */
  title: string;
  /** A description of the scenario content and objectives */
  description: string;
  /** The difficulty level of the scenario */
  difficulty: DifficultyLevel;
  /** The expected duration of a session */
  sessionLength: SessionLength;
  /** Optional list of tags for categorization */
  tags?: string[];
  /** Optional list of character archetypes available in this scenario */
  archetypes?: string[];
  /** The target age group for this scenario */
  ageGroup: string;
  /** The minimum recommended age for players */
  minimumAge: number;
  /** Optional list of core moral compass axes explored in this scenario */
  coreAxes?: string[];
  /** Optional list of characters in this scenario */
  characters?: CharacterRequest[];
  /** Optional list of scenes in this scenario */
  scenes?: SceneRequest[];
  /** Optional URL or identifier for the scenario's cover image */
  image?: string;
  /** Optional list of compass axes used in this scenario */
  compassAxes?: string[];
}

/**
 * Request to query scenarios with filtering and pagination
 */
export interface ScenarioQueryRequest {
  /** Optional filter by difficulty level */
  difficulty?: DifficultyLevel;
  /** Optional filter by session length */
  sessionLength?: SessionLength;
  /** Optional filter by minimum age requirement */
  minimumAge?: number;
  /** Optional filter by age group */
  ageGroup?: string;
  /** Optional list of tags to filter scenarios */
  tags?: string[];
  /** The page number for pagination (1-based) */
  page?: number;
  /** The number of items per page */
  pageSize?: number;
  /** Optional search term to filter scenarios by title or description */
  searchTerm?: string;
  /** Optional search query (alias for searchTerm) */
  search?: string;
  /** Optional filter by genre */
  genre?: string;
  /** Optional list of archetypes to filter scenarios */
  archetypes?: string[];
  /** Optional list of core compass axes to filter scenarios */
  coreAxes?: string[];
}

// =============================================================================
// Game Session Request Types
// =============================================================================

/**
 * Player assignment to a character
 */
export interface PlayerAssignmentDto {
  /** The type of player assignment (e.g., Profile, Guest, NPC) */
  type?: string;
  /** Optional profile identifier if assigned to a registered profile */
  profileId?: string;
  /** Optional profile name if assigned to a registered profile */
  profileName?: string;
  /** Optional URL or identifier for the profile's image */
  profileImage?: string;
  /** Optional identifier for the selected avatar media */
  selectedAvatarMediaId?: string;
  /** Optional guest name if assigned to a guest player */
  guestName?: string;
  /** Optional age range for guest players */
  guestAgeRange?: string;
  /** Optional avatar identifier for guest players */
  guestAvatar?: string;
  /** Indicates whether to save the guest as a new profile */
  saveAsProfile?: boolean;
}

/**
 * Character assignment in a game session
 */
export interface CharacterAssignmentDto {
  /** The unique identifier of the character */
  characterId?: string;
  /** The display name of the character */
  characterName?: string;
  /** Optional URL or identifier for the character's image */
  image?: string;
  /** Optional URL or identifier for the character's audio */
  audio?: string;
  /** The role of the character in the story */
  role?: string;
  /** The archetype classification of the character */
  archetype?: string;
  /** Indicates whether this character is not assigned to any player */
  isUnused?: boolean;
  /** Optional player assignment information for this character */
  playerAssignment?: PlayerAssignmentDto;
}

/**
 * Request to start a new game session
 */
export interface StartGameSessionRequest {
  /** The unique identifier of the scenario to play */
  scenarioId: string;
  /** The account identifier of the user starting the session */
  accountId: string;
  /** The profile identifier of the player */
  profileId: string;
  /** Optional list of player names participating in the session */
  playerNames?: string[];
  /** Optional list of character assignments for each player */
  characterAssignments?: CharacterAssignmentDto[];
  /** The target age group for content filtering */
  targetAgeGroup: string;
}

/**
 * Request to make a choice during a game session
 */
export interface MakeChoiceRequest {
  /** The unique identifier of the current session */
  sessionId: string;
  /** The unique identifier of the current scene */
  sceneId: string;
  /** The text of the choice made by the player */
  choiceText: string;
  /** The unique identifier of the next scene to navigate to */
  nextSceneId: string;
  /** Optional identifier of the player making the choice */
  playerId?: string;
  /** Optional compass axis affected by this choice */
  compassAxis?: string;
  /** Optional direction on the compass axis */
  compassDirection?: string;
  /** Optional delta value for compass movement */
  compassDelta?: number;
}

/**
 * Request to progress to the next scene in a session
 */
export interface ProgressSceneRequest {
  /** The unique identifier of the current session */
  sessionId: string;
  /** The unique identifier of the scene to progress to */
  sceneId: string;
}

/**
 * Request to complete a scenario session
 */
export interface CompleteScenarioRequest {
  /** The unique identifier of the session to complete */
  sessionId: string;
  /** The unique identifier of the account */
  accountId: string;
  /** The unique identifier of the scenario */
  scenarioId: string;
  /** Optional final scores for compass axes */
  finalScores?: Record<string, number>;
}

// =============================================================================
// Scenario Response Types
// =============================================================================

/**
 * Summary information for a scenario
 */
export interface ScenarioSummary {
  /** The unique identifier of the scenario */
  id: string;
  /** The title of the scenario */
  title: string;
  /** A description of the scenario */
  description: string;
  /** The target age group for this scenario */
  ageGroup: string;
  /** The difficulty level of the scenario */
  difficulty: string;
  /** Optional list of tags for categorization */
  tags?: string[];
  /** The expected duration of a session */
  sessionLength?: string;
  /** Optional list of character archetypes available in this scenario */
  archetypes?: string[];
  /** The minimum recommended age for players */
  minimumAge?: number;
  /** Optional list of core moral compass axes explored in this scenario */
  coreAxes?: string[];
  /** The date and time when the scenario was created (ISO 8601) */
  createdAt: string;
  /** Optional music palette identifier for the scenario */
  musicPalette?: string;
}

/**
 * Response containing a paginated list of scenarios
 */
export interface ScenarioListResponse {
  /** The list of scenario summaries */
  scenarios: ScenarioSummary[];
  /** The total number of scenarios matching the query */
  totalCount: number;
  /** The current page number */
  page: number;
  /** The number of items per page */
  pageSize: number;
  /** Indicates if there are more pages available */
  hasNextPage: boolean;
}

/**
 * Scenario information including current game state
 */
export interface ScenarioWithGameState {
  /** The unique identifier of the scenario */
  scenarioId: string;
  /** The title of the scenario */
  title: string;
  /** A description of the scenario */
  description: string;
  /** The current game state (NotStarted, InProgress, Completed) */
  gameState: string;
  /** The date and time when the scenario was last played (ISO 8601) */
  lastPlayedAt?: string;
  /** The target age group for this scenario */
  ageGroup?: string;
  /** The difficulty level of the scenario */
  difficulty?: string;
  /** The expected duration of a session */
  sessionLength?: string;
  /** Optional list of core moral compass axes explored in this scenario */
  coreAxes?: string[];
  /** Optional list of tags for categorization */
  tags?: string[];
  /** Optional list of character archetypes available in this scenario */
  archetypes?: string[];
  /** The number of times this scenario has been played */
  playCount: number;
  /** Optional URL or identifier for the scenario's cover image */
  image?: string;
}

/**
 * Response containing scenarios with their game state information
 */
export interface ScenarioGameStateResponse {
  /** The list of scenarios with game state information */
  scenarios: ScenarioWithGameState[];
  /** The total number of scenarios */
  totalCount: number;
}

// =============================================================================
// Game Session Response Types
// =============================================================================

/**
 * Player compass progress data
 */
export interface PlayerCompassProgressDto {
  /** Player identifier */
  playerId: string;
  /** Player name */
  playerName: string;
  /** Compass axis values */
  compassValues: Record<string, number>;
}

/**
 * Response containing game session information
 */
export interface GameSessionResponse {
  /** The unique identifier of the session */
  id: string;
  /** The unique identifier of the scenario being played */
  scenarioId: string;
  /** The account identifier of the session owner */
  accountId: string;
  /** The profile identifier of the primary player */
  profileId: string;
  /** The list of player names participating in the session */
  playerNames: string[];
  /** The current status of the session (e.g., Active, Paused, Completed) */
  status: string;
  /** The unique identifier of the current scene */
  currentSceneId: string;
  /** The total number of choices made in this session */
  choiceCount: number;
  /** The date and time when the session started (ISO 8601) */
  startTime: string;
  /** The date and time when the session ended, if completed (ISO 8601) */
  endTime?: string;
  /** The target age group for content filtering in this session */
  targetAgeGroup: string;
  /** Compass progress totals for all players in the session */
  playerCompassProgressTotals?: PlayerCompassProgressDto[];
  /** The number of echoes earned in this session */
  echoCount: number;
  /** The number of achievements earned in this session */
  achievementCount: number;
  /** The elapsed time of the session (ISO 8601 duration format) */
  elapsedTime?: string;
  /** Indicates whether the session is currently paused */
  isPaused: boolean;
  /** The total number of scenes in the session */
  sceneCount: number;
  /** The character assignments for this session */
  characterAssignments?: CharacterAssignmentDto[];
}

/**
 * Response containing session statistics
 */
export interface SessionStatsResponse {
  /** Dictionary of compass axis values accumulated during the session */
  compassValues: Record<string, number>;
  /** The total number of choices made during the session */
  totalChoices: number;
  /** The total duration of the session (ISO 8601 duration format) */
  sessionDuration: string;
  /** Compass progress totals for all players in the session */
  playerCompassProgressTotals?: PlayerCompassProgressDto[];
  /** Recent echoes earned during the session */
  recentEchoes?: unknown[];
  /** Achievements earned during the session */
  achievements?: unknown[];
}

// =============================================================================
// Badge Response Types
// =============================================================================

/**
 * Represents a badge that can be earned by users
 */
export interface BadgeResponse {
  /** Unique identifier for the badge */
  id: string;
  /** Display name of the badge */
  name: string;
  /** Description of how to earn the badge */
  description: string;
  /** URL to the badge icon/image */
  iconUrl?: string;
  /** Category or type of the badge (e.g., "story", "engagement", "milestone") */
  category?: string;
  /** The tier of this badge (e.g., "bronze", "silver", "gold", "platinum") */
  tier?: string;
  /** Display order of this tier within the badge hierarchy */
  tierOrder: number;
  /** Title of the badge */
  title?: string;
  /** Age group identifier for age-appropriate badge tracking */
  ageGroupId?: string;
  /** Required score threshold to earn this badge */
  requiredScore: number;
  /** Media identifier for the badge image */
  imageId?: string;
  /** Points awarded when earning this badge */
  points: number;
  /** Whether the user has earned this badge */
  isEarned: boolean;
  /** When the badge was earned, if applicable (ISO 8601) */
  earnedAt?: string;
  /** The compass axis this badge is associated with, if any */
  compassAxisId?: string;
}

/**
 * Represents an achievement earned on a specific compass axis
 */
export interface AxisAchievementResponse {
  /** Unique identifier for the achievement */
  id: string;
  /** Age group identifier for age-appropriate achievement tracking */
  ageGroupId?: string;
  /** The compass axis this achievement belongs to */
  compassAxisId: string;
  /** Name of the compass axis */
  compassAxisName?: string;
  /** Name of the axis (e.g., "Creativity", "Logic", "Empathy") */
  axisName: string;
  /** Direction on the compass axis (e.g., "positive", "negative") */
  axesDirection?: string;
  /** Display name of the achievement */
  name: string;
  /** Description of the achievement */
  description: string;
  /** Level or tier of this achievement within the axis */
  level: number;
  /** Score threshold required to unlock this achievement */
  scoreThreshold: number;
  /** URL to the achievement icon */
  iconUrl?: string;
  /** Whether the user has unlocked this achievement */
  isUnlocked: boolean;
  /** When the achievement was unlocked, if applicable (ISO 8601) */
  unlockedAt?: string;
}

/**
 * Represents progress through badge tiers
 */
export interface BadgeTierProgressResponse {
  /** Unique identifier for the badge */
  badgeId?: string;
  /** The tier name (e.g., "bronze", "silver", "gold", "platinum") */
  tierName: string;
  /** Tier identifier (e.g., "bronze", "silver", "gold") */
  tier?: string;
  /** Display order of this tier */
  tierOrder: number;
  /** Title of the badge tier */
  title?: string;
  /** Description of the badge tier */
  description?: string;
  /** Required score threshold to earn this tier */
  requiredScore: number;
  /** Media identifier for the tier image */
  imageId?: string;
  /** Whether the tier has been earned */
  isEarned: boolean;
  /** When the tier was earned, if applicable (ISO 8601) */
  earnedAt?: string;
  /** Progress toward the threshold as a percentage (0-1) */
  progressToThreshold: number;
  /** Remaining score needed to reach this tier */
  remainingScore: number;
  /** Total badges available in this tier */
  totalBadges: number;
  /** Number of badges earned in this tier */
  earnedBadges: number;
  /** Badges in this tier */
  badges: BadgeResponse[];
  /** Color associated with this tier for UI display */
  color?: string;
  /** Icon URL for this tier */
  iconUrl?: string;
}

/**
 * Represents progress on a specific compass axis
 */
export interface AxisProgressResponse {
  /** The compass axis identifier */
  compassAxisId: string;
  /** Name of the compass axis */
  compassAxisName?: string;
  /** Display name of the axis */
  axisName: string;
  /** Current score on this axis */
  currentScore: number;
  /** Maximum possible score on this axis */
  maxScore: number;
  /** Current level achieved on this axis */
  currentLevel: number;
  /** Score needed to reach the next level */
  nextLevelThreshold?: number;
  /** Progress percentage toward next level (0-100) */
  nextLevelProgress?: number;
  /** Achievements earned on this axis */
  achievements: AxisAchievementResponse[];
  /** Badge tier progress for this axis */
  tiers: BadgeTierProgressResponse[];
  /** Color associated with this axis for UI display */
  color?: string;
}

/**
 * Represents progress toward earning a badge
 */
export interface BadgeProgressResponse {
  /** The badge being tracked */
  badge?: BadgeResponse;
  /** Current progress value */
  currentValue: number;
  /** Target value to earn the badge */
  targetValue: number;
  /** Description of the current milestone */
  milestoneDescription?: string;
  /** The age group identifier for age-appropriate badge tracking */
  ageGroupId?: string;
  /** Progress on each compass axis related to this badge */
  axisProgresses: AxisProgressResponse[];
}

/**
 * Represents a user's score on a compass axis
 */
export interface CompassAxisScoreResult {
  /** The compass axis identifier */
  compassAxisId: string;
  /** Display name of the axis */
  axisName: string;
  /** The calculated score for this axis */
  score: number;
  /** Normalized score (0-1 range) */
  normalizedScore: number;
  /** Percentile rank compared to other users */
  percentile?: number;
  /** Description of what this score means */
  interpretation?: string;
  /** Strength level based on score (e.g., "low", "medium", "high", "exceptional") */
  strengthLevel?: string;
  /** Related traits or characteristics for this axis */
  relatedTraits: string[];
  /** When this score was last calculated (ISO 8601) */
  calculatedAt: string;
}

// =============================================================================
// Re-exports for backwards compatibility
// =============================================================================

export type {
  ApiRequest as BaseRequest,
  ApiResponse as BaseResponse,
};
