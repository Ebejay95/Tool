// Site-specific JavaScript functionality
console.log('CMC Web Application loaded');

window.cookieHelper = {
    get: function (name) {
        const match = document.cookie.split('; ').find(r => r.startsWith(name + '='));
        return match ? match.split('=')[1] : null;
    },
    set: function (name, value, days) {
        const maxAge = days * 24 * 60 * 60;
        document.cookie = `${name}=${value}; path=/; max-age=${maxAge}; SameSite=Lax`;
    }
};
