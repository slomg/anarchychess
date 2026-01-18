import { ReactNode } from "react";

import ChallengeNotificationRenderer from "@/features/challenges/components/challengeNotification/ChallengeNotificationRenderer";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import LobbyHandler from "@/features/lobby/components/LobbyHandler";
import NavDesktop from "@/features/navbar/components/NavDesktop";
import NavMobile from "@/features/navbar/components/NavMobile";

export default function PublicLayout({ children }: { children: ReactNode }) {
    return (
        <SessionProvider user={null}>
            <div
                className="flex min-h-screen max-w-screen min-w-[300px] flex-col
                    md:flex-row"
            >
                <NavMobile />
                <NavDesktop />
                {children}
            </div>

            <LobbyHandler />
            <ChallengeNotificationRenderer />
        </SessionProvider>
    );
}
