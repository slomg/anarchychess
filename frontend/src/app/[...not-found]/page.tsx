import { notFound } from "next/navigation";
import { Metadata } from "next";

export const metadata: Metadata = {
    title: "Not Found",
};

export default function catchAll() {
    return notFound();
}
