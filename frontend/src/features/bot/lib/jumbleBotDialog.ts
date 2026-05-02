import { BotDialogOptions, ReactionDialog } from "../hooks/useBotDialog";

interface JumbleIntensity {
    wordIntensity: number;
    letterIntensity: number;
}

export function jumbleDialogOptions(
    dialogOptions: BotDialogOptions,
    JumbleIntensity: JumbleIntensity,
): BotDialogOptions {
    return {
        reactionDialog: jumbleReactionDialog(
            dialogOptions.reactionDialog,
            JumbleIntensity,
        ),
        generalDialog: jumbleDialog(
            dialogOptions.generalDialog,
            JumbleIntensity,
        ),
        startDialog: jumbleDialog(dialogOptions.startDialog, JumbleIntensity),
        botWinDialog: jumbleDialog(dialogOptions.botWinDialog, JumbleIntensity),
        botLoseDialog: jumbleDialog(
            dialogOptions.botLoseDialog,
            JumbleIntensity,
        ),
    };
}

export function jumbleDialog(
    dialogs: string[],
    JumbleIntensity: JumbleIntensity,
): string[] {
    const result: string[] = [];
    for (const dialog of dialogs) {
        result.push(jumbleText(dialog, JumbleIntensity));
    }
    return result;
}

export function jumbleReactionDialog(
    dialogs: ReactionDialog[],
    JumbleIntensity: JumbleIntensity,
): ReactionDialog[] {
    const result: ReactionDialog[] = [];
    for (const dialog of dialogs) {
        const newLines: string[] = jumbleDialog(dialog.lines, JumbleIntensity);
        result.push({ condition: dialog.condition, lines: newLines });
    }
    return result;
}

function jumbleText(
    dialog: string,
    { wordIntensity, letterIntensity }: JumbleIntensity,
): string {
    const match = dialog.match(/[a-zA-Z]+|[^a-zA-Z]+/g);
    if (match === null) {
        return "";
    }

    const tokens = [...match];
    const result: string[] = [];

    for (const token of tokens) {
        if (/^[A-Za-z]*$/.test(token) && Math.random() < wordIntensity) {
            result.push(jumbleWord(token, letterIntensity));
        } else {
            result.push(token);
        }
    }

    console.log(result);
    return result.join("");
}

function jumbleWord(word: string, intensity: number): string {
    if (word.length <= 3) {
        return word;
    }

    const first = word[0];
    const last = word[word.length - 1];
    const middle = word.slice(1, -1).split("");

    for (let i = middle.length - 1; i > 0; i--) {
        if (Math.random() < intensity) {
            const j = Math.floor(Math.random() * (i + 1));
            [middle[i], middle[j]] = [middle[j], middle[i]];
        }
    }

    return first + middle.join("") + last;
}
