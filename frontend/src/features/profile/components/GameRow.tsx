import {
    PlusCircleIcon,
    MinusCircleIcon,
    PauseCircleIcon,
    CpuChipIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";
import clsx from "clsx";

import TimeControlIconFromSeconds from "@/features/lobby/components/TimeControlIconFromSeconds";
import { GameResult, GameSummary, PublicUser } from "@/lib/apiClient";
import ProfileTooltip from "./ProfileTooltip";
import constants from "@/lib/constants";

const GameRow = ({
    game,
    profileViewpoint,
    index,
}: {
    game: GameSummary;
    profileViewpoint: PublicUser;
    index: number;
}) => {
    const winCondition =
        game.whitePlayer.userId === profileViewpoint.userId
            ? GameResult.WHITE_WIN
            : GameResult.BLACK_WIN;

    const isDraw = game.result === GameResult.DRAW;
    const isWinner = game.result === winCondition;

    // Format the game date
    const formattedDate = new Date(game.createdAt).toLocaleDateString("en-us", {
        month: "short",
        day: "numeric",
        year: "numeric",
    });

    // Find the results icon (whether the profile author is the winner or it's a draw)
    // and find the score for each color
    const ResultsIcon = isDraw ? (
        <PauseCircleIcon className="text-gray-500" />
    ) : isWinner ? (
        <PlusCircleIcon className="text-green-400" />
    ) : (
        <MinusCircleIcon className="text-red-400" />
    );

    function getScore(winResult: GameResult): string {
        if (isDraw) return "½";

        return game.result === winResult ? "1" : "0";
    }

    return (
        <tr
            data-testid={`gameRow-${game.gameToken}`}
            className={clsx(
                index % 2 === 0 ? "bg-gray-400/5" : "bg-gray-600/5",
                "whitespace-nowrap",
            )}
        >
            <td className="relative">
                <GameLink gameToken={game.gameToken} isBot={game.isBotGame} />

                <div
                    className="flex flex-col items-center justify-center gap-1
                        px-3"
                >
                    {game.isBotGame ? (
                        <CpuChipIcon
                            className="h-8 w-8"
                            data-testid="gameRowBotIcon"
                        />
                    ) : (
                        <>
                            <TimeControlIconFromSeconds
                                baseSeconds={game.baseSeconds}
                                className="h-8 w-8"
                            />
                            <span
                                className="text-xl"
                                data-testid="gameRowTimeControlText"
                            >
                                {(game.baseSeconds / 60)
                                    .toString()
                                    .replace(/^0/, "")}
                                +{game.incrementSeconds}
                            </span>
                        </>
                    )}
                </div>
            </td>
            <td className="relative flex">
                <GameLink gameToken={game.gameToken} isBot={game.isBotGame} />

                <div className="relative flex flex-col justify-center py-4">
                    <ProfileTooltip userId={game.whitePlayer.userId}>
                        <p data-testid="gameRowWhiteUsername">
                            {game.whitePlayer.userName}
                        </p>
                    </ProfileTooltip>
                    <ProfileTooltip userId={game.blackPlayer.userId}>
                        <p
                            className="text-text/50"
                            data-testid="gameRowBlackUsername"
                        >
                            {game.blackPlayer.userName}
                        </p>
                    </ProfileTooltip>
                </div>
            </td>

            <td className="relative">
                <GameLink gameToken={game.gameToken} isBot={game.isBotGame} />
                <div className="flex items-center gap-3">
                    <div className="flex w-3 flex-col justify-between">
                        <span data-testid="gameRowScoreWhite">
                            {getScore(GameResult.WHITE_WIN)}
                        </span>

                        <span data-testid="gameRowScoreBlack">
                            {getScore(GameResult.BLACK_WIN)}
                        </span>
                    </div>
                    <span className="size-7">{ResultsIcon}</span>
                </div>
            </td>

            <td className="relative">
                <GameLink gameToken={game.gameToken} isBot={game.isBotGame} />
                <span data-testid="gameRowDate">{formattedDate}</span>
            </td>
        </tr>
    );
};
export default GameRow;

const GameLink = ({
    gameToken,
    isBot,
}: {
    gameToken: string;
    isBot: boolean;
}) => (
    <Link
        data-testid="gameRowLink"
        className="absolute inset-0"
        href={`${isBot ? constants.PATHS.BOT : constants.PATHS.GAME}/${gameToken}`}
    />
);
