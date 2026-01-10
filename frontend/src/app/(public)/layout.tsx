import { ReactNode } from "react";

import ChallengeNotificationRenderer from "@/features/challenges/components/challengeNotification/ChallengeNotificationRenderer";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import LobbyHandler from "@/features/lobby/components/LobbyHandler";
import Navbar from "@/features/navbar/components/Navbar";

export default function PublicLayout({ children }: { children: ReactNode }) {
    return (
        <SessionProvider user={null}>
            <div
                className="flex min-h-screen max-w-screen min-w-[300px] flex-col
                    md:flex-row"
            >
                <Navbar />
                {children}
            </div>

            <LobbyHandler />
            <ChallengeNotificationRenderer />
        </SessionProvider>
    );
}
