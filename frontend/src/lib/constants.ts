import {
    PieceType,
    TimeControl,
    TimeControlSettings,
} from "./apiClient/definition/types.gen";

const USERNAME_EDIT_EVERY_MS = 1000 * 60 * 60 * 24 * 7 * 2;

const BOARD_WIDTH = 10;
const BOARD_HEIGHT = 10;

const PROFILE_PICTURE_MAX_SIZE = 1024 * 1024 * 2;

export enum OAuthProvider {
    GOOGLE = "google",
    DISCORD = "discord",
}

const COOKIES = {
    REFRESH_TOKEN: "refreshToken",
    ACCESS_TOKEN: "accessToken",
    IS_LOGGED_IN: "isLoggedIn",
    AUTH_FAILURE: "authFailed",
};

const PATHS = {
    SIGNIN: "/signin",
    LOGOUT: "/logout",
    REFRESH: "/refresh",
    GAME: "/game",
    BOT: "/bot",
    BOT_OFFLINE: "/bot/offline",
    PLAY: "/play",
    GUIDE: "/guide",
    QUESTS: "/quests",
    PROFILE: "/profile",
    ANALYSIS: "/analysis",
    CHALLENGE: "/challenge",
    WIN_STREAK: "/win-streak",
    SETTINGS_BASE: "/settings",
    SETTINGS_PROFILE: "/settings/profile",
    SETTINGS_SOCIAL: "/settings/social",
    OAUTH: `${process.env.NEXT_PUBLIC_OAUTH_URL}/api/oauth/signin/`,
    TOS: "/tos",
    PRIVACY: "/privacy",
    DONATE: "https://ko-fi.com/anarchychess",
    GITHUB: "https://github.com/slomg/anarchychess",
    DISCORD: "https://discord.gg/qnkddndecq",
    YOUTUBE: "https://youtube.com/@slomgdev",
    REDDIT: "https://reddit.com/user/Slim_Bun",
};

const DISALLOW_AUTH_PATHS: ReadonlySet<string> = new Set<string>([
    PATHS.SIGNIN,
]);

const LOCALSTORAGE = {
    PREFERS_MATCHMAKING_POOL: "prefersMatchmakingPool",
    PREFERS_CHALLENGE_POOL: "prefersChallengePool",
    PREFERS_TIME_CONTROL_MINUTES_IDX: "prefersTimeControlMinutesIdx",
    PREFERS_TIME_CONTROL_INCREMENT_IDX: "prefersTimeControlIncrementIdx",
    IS_SIDEBAR_COLLAPSED: "isSidebarCollapsed",
    PREFERS_BOT_TYPE: "prefersBotType",
};

const SIGNALR_PATHS = {
    LOBBY: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/lobby`,
    OPENSEEK: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/openseek`,
    GAME: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/game`,
    BOT: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/bot`,
    CHALLENGE: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/challenge`,
};

const PAGINATION_PAGE_SIZE = {
    GAME_SUMMARY: 10,
    QUEST_LEADERBOARD: 10,
    WIN_STREAK_LEADERBOARD: 10,
    STARS: 5,
    BLOCKED: 5,
};

interface TimeControlLabel {
    settings: TimeControlSettings;
    isMostPopular?: boolean;
}

const STANDARD_TIME_CONTROLS: TimeControlLabel[] = [
    {
        settings: {
            baseSeconds: 60,
            incrementSeconds: 0,
            type: TimeControl.BULLET,
        },
    },
    {
        settings: {
            baseSeconds: 120,
            incrementSeconds: 1,
            type: TimeControl.BULLET,
        },
    },
    {
        settings: {
            baseSeconds: 180,
            incrementSeconds: 0,
            type: TimeControl.BLITZ,
        },
    },
    {
        settings: {
            baseSeconds: 180,
            incrementSeconds: 2,
            type: TimeControl.BLITZ,
        },
    },
    {
        settings: {
            baseSeconds: 300,
            incrementSeconds: 0,
            type: TimeControl.BLITZ,
        },
        isMostPopular: true,
    },
    {
        settings: {
            baseSeconds: 300,
            incrementSeconds: 3,
            type: TimeControl.BLITZ,
        },
    },
    {
        settings: {
            baseSeconds: 600,
            incrementSeconds: 0,
            type: TimeControl.RAPID,
        },
    },
    {
        settings: {
            baseSeconds: 900,
            incrementSeconds: 10,
            type: TimeControl.RAPID,
        },
    },
    {
        settings: {
            baseSeconds: 1800,
            incrementSeconds: 0,
            type: TimeControl.CLASSICAL,
        },
    },
];

const CHALLENGE_MINUTES_OPTIONS = [
    0.25, 0.5, 1, 2, 3, 5, 7, 10, 15, 20, 25, 30, 45, 60, 90,
];
const DEFAULT_CHALLENGE_MINUTE_OPTION_IDX = 5;

const CHALLENGE_INCREMENT_SECONDS_OPTIONS = [
    0, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 60,
];
const DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX = 0;

const TIME_CONTROL_LABELS: Record<TimeControl, string> = {
    [TimeControl.BULLET]: "Bullet",
    [TimeControl.BLITZ]: "Blitz",
    [TimeControl.RAPID]: "Rapid",
    [TimeControl.CLASSICAL]: "Classical",
};

const DISPLAY_TIME_CONTROLS: TimeControl[] = [
    TimeControl.BULLET,
    TimeControl.BLITZ,
    TimeControl.RAPID,
    TimeControl.CLASSICAL,
];

const SEEK_RESUBSCRIBE_INTERAVAL_MS = 1000 * 60 * 4;
const OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS = 1000 * 60 * 4;

const ALLOW_ABORTION_UNTIL_MOVE = 1;

const INITIAL_FEN =
    "rhnbqkbcar/pppdppdppp/10/+9/10/10/9+/10/PPPDPPDPPP/RHNBQKBCAR";

const LETTER_TO_PIECE: Record<string, PieceType> = {
    k: PieceType.KING,
    q: PieceType.QUEEN,
    r: PieceType.ROOK,
    n: PieceType.KNOOK,
    b: PieceType.BISHOP,
    h: PieceType.HORSEY,
    p: PieceType.PAWN,
    d: PieceType.UNDERAGE_PAWN,
    a: PieceType.ANTIQUEEN,
    "+": PieceType.TRAITOR_ROOK,
    c: PieceType.CHECKER,
};

const ANIMATION_STEP_DELAY_MS = 30;
const PIECE_ANIMATION_LENGTH_MS = 100;

const QUEST_WEEKDAY_NAMES: Record<number, string> = {
    0: "Savage Sunday",
    1: "Mundane Monday",
    2: "Tame Tuesday",
    3: "Wild Wednesday",
    4: "Thrilling Thursday",
    5: "Fiery Friday",
    6: "Strenuous Saturday",
};

const constants = {
    USERNAME_EDIT_EVERY_MS,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    PROFILE_PICTURE_MAX_SIZE,
    STANDARD_TIME_CONTROLS,
    TIME_CONTROL_LABELS,
    DISPLAY_TIME_CONTROLS,
    CHALLENGE_MINUTES_OPTIONS,
    DEFAULT_CHALLENGE_MINUTE_OPTION_IDX,
    CHALLENGE_INCREMENT_SECONDS_OPTIONS,
    DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX,
    SEEK_RESUBSCRIBE_INTERAVAL_MS,
    OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
    COOKIES,
    PATHS,
    DISALLOW_AUTH_PATHS,
    LOCALSTORAGE,
    ALLOW_ABORTION_UNTIL_MOVE,
    INITIAL_FEN,
    LETTER_TO_PIECE,
    SIGNALR_PATHS,
    PAGINATION_PAGE_SIZE,
    QUEST_WEEKDAY_NAMES,
    ANIMATION_STEP_DELAY_MS,
    PIECE_ANIMATION_LENGTH_MS,
} as const;
export default constants;
