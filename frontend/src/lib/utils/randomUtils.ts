export function randomItem<T>(arr: T[]): T | null {
    if (arr.length === 0) {
        return null;
    }

    const idx = Math.floor(Math.random() * arr.length);
    return arr[idx];
}

export function seededRandomItem<T>(arr: T[], seed: string): T | null {
    if (arr.length === 0) {
        return null;
    }

    let hash = 0;
    for (let i = 0; i < seed.length; i++) {
        hash = (hash * 31 + seed.charCodeAt(i)) >>> 0;
    }
    const idx = hash % arr.length;
    return arr[idx];
}
