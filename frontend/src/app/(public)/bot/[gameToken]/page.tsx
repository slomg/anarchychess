import WithSession from "@/features/auth/hocs/WithSession";
import BotChessboard from "@/features/bot/components/BotChessboard";
import { getBotGame } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Bot Game - Anarchy Chess" };

export default async function BotGamePage({
    params,
}: {
    params: Promise<{ gameToken: string }>;
}) {
    return (
        <WithSession>
            {async ({ accessToken }) => {
                const { gameToken } = await params;

                const game = await dataOrThrow(
                    getBotGame({
                        path: { gameToken },
                        auth: accessToken,
                    }),
                );

                return <BotChessboard gameToken={gameToken} gameState={game} />;
            }}
        </WithSession>
    );
}
