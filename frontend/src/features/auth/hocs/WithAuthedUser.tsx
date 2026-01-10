import SessionProvider from "@/features/auth/contexts/sessionContext";
import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { fetchAuthedUserSession } from "../lib/getLoggedIn";
import AuthRefresh from "../components/AuthRefresh";
import { type PrivateUser } from "@/lib/apiClient";

interface WithAuthedUserProps {
    user: PrivateUser;
    accessToken: string;
}

export default async function WithAuthedUser({
    children,
}: {
    children: Renderable<WithAuthedUserProps>;
}) {
    const session = await fetchAuthedUserSession();
    if (!session) return <AuthRefresh />;

    return (
        <SessionProvider user={session.user} fetchAttempted>
            {renderRenderable(children, session)}
        </SessionProvider>
    );
}
