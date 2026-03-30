window.sessionAuth = {
    login: async function (url, username, password) {
        const response = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });

        const payload = await response.json().catch(() => null);
        if (payload) {
            return payload;
        }

        return {
            succeeded: response.ok,
            errorMessage: response.ok ? '' : 'Authentication request failed.',
            successMessage: response.ok ? 'Authentication request succeeded.' : null
        };
    },

    logout: async function (url) {
        await fetch(url, {
            method: 'POST',
            credentials: 'same-origin'
        });
    }
};