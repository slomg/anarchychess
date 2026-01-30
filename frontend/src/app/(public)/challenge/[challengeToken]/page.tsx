import constants from "@/lib/constants";

import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import ChallengeSidebar from "@/features/challenges/components/challengePageSidebar/ChallengeSidebar";
import WithSession from "@/features/auth/hocs/WithSession";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import { getChallenge } from "@/lib/apiClient";
import { redirect } from "next/navigation";

export const metadata = { title: "Challenge - Anarchy Chess" };

export default async function ChallengePage({
    params,
}: {
    params: Promise<{ challengeToken: string }>;
}) {
    const { challengeToken } = await params;

    return (
        <WithSession>
            {async ({ accessToken }) => {
                const challenge = await dataOrThrow(
                    getChallenge({
                        path: { challengeToken },
                        auth: () => accessToken,
                    }),
                );

                if (challenge.resolvedGame != null) {
                    redirect(
                        `${constants.PATHS.GAME}/${challenge.resolvedGame}`,
                    );
                }

                return (
                    <StaticChessboardWithSidebar
                        prioritizeAside
                        aside={<ChallengeSidebar challenge={challenge} />}
                    />
                );
            }}
        </WithSession>
    );
}
