import { Metadata } from "next";

import LogoutHandler from "@/features/auth/components/LogoutHandler";

export const metadata: Metadata = {
    title: "Log Out - Anarchy Chess",
};

const LogoutPage = () => {
    return <LogoutHandler />;
};

export default LogoutPage;
