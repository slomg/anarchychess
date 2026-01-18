import { useEffect, useEffectEvent, useState } from "react";
import Cookies from "js-cookie";

export default function useCookieValue<T>(
    cookieName: string,
    defaultValue: T,
): T | undefined {
    const [value, setValue] = useState<T | undefined>();

    const setValueFromCookie = useEffectEvent(() => {
        const cookieValue = Cookies.get(cookieName);
        if (!cookieValue) {
            setValue(defaultValue);
            return;
        }
        setValue(JSON.parse(cookieValue));
    });
    useEffect(() => setValueFromCookie(), []);

    return value;
}
