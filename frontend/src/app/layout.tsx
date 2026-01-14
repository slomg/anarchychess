import { SpeedInsights } from "@vercel/speed-insights/next";
import { Analytics } from "@vercel/analytics/next";
import { Secular_One } from "next/font/google";
import { ReactNode } from "react";

import clsx from "clsx";
import "./globals.css";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export default async function RootLayout({
    children,
}: {
    children: ReactNode;
}) {
    return (
        <html lang="en" data-bs-theme="dark" data-scroll-behavior="smooth">
            <body
                className={clsx(
                    "bg-background text-text",
                    secularOne.className,
                )}
            >
                <Analytics />
                <SpeedInsights />
                {children}
            </body>
        </html>
    );
}
