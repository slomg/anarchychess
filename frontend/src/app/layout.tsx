import { SpeedInsights } from "@vercel/speed-insights/next";
import { Secular_One } from "next/font/google";
import { ReactNode } from "react";
import { Metadata } from "next";

import clsx from "clsx";
import "./globals.css";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export const metadata: Metadata = {
    icons: {
        icon: "./favicon.ico",
    },
};

export default async function RootLayout({
    children,
}: {
    children: ReactNode;
}) {
    return (
        <html lang="en" data-bs-theme="dark">
            <body
                className={clsx(
                    "bg-background text-text",
                    secularOne.className,
                )}
            >
                <SpeedInsights />
                {children}
            </body>
        </html>
    );
}
