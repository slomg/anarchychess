import { Metadata } from "next";

import Button from "@/components/ui/Button";
import Link from "next/link";

export const metadata: Metadata = {
    title: "Not Found - Anarchy Chess",
};

export default function NotFound() {
    return (
        <div
            className="flex h-screen w-screen flex-1 flex-col items-center
                justify-center gap-5"
        >
            <h1 className="text-3xl">Page not found</h1>
            <Button>
                <Link href="/">Go back to Home</Link>
            </Button>
        </div>
    );
}
