// Theme sofort anwenden – verhindert weißes Aufblitzen bei Dark Mode
// Dieses Script wird synchron im <head> geladen (kein async/defer)
(function () {
    var match = document.cookie.split('; ').find(function (r) {
        return r.startsWith('theme=');
    });
    if (match && match.split('=')[1] === 'dark') {
        document.documentElement.style.backgroundColor = '#121212';
        document.documentElement.style.colorScheme = 'dark';
    }
})();
