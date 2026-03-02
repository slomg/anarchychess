import { GameColor } from "../apiClient";

export function invertColor(color: GameColor): GameColor {
    return color === GameColor.WHITE ? GameColor.BLACK : GameColor.WHITE;
}

export function randomizeColor(): GameColor {
    return Math.random() < 0.5 ? GameColor.WHITE : GameColor.BLACK;
}
