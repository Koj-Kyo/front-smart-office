// Gestionnaire global d'erreurs JavaScript pour Blazor
window.blazorErrorHandler = {
    // Intercepter les erreurs JavaScript non gérées
    handleError: function (error, errorInfo) {
        console.error('?? Erreur JavaScript interceptée:', error, errorInfo);
        
        // Masquer la barre d'erreur après quelques secondes
        setTimeout(function() {
            const errorUI = document.getElementById('blazor-error-ui');
            if (errorUI) {
                errorUI.style.display = 'none';
            }
        }, 5000);
        
        return false; // Ne pas propager l'erreur
    },

    // Initialiser le gestionnaire d'erreurs
    initialize: function() {
        // Intercepter les erreurs JavaScript globales
        window.addEventListener('error', function(event) {
            console.warn('?? Erreur JavaScript globale:', event.error);
            window.blazorErrorHandler.handleError(event.error, {
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno
            });
        });

        // Intercepter les promesses rejetées non gérées
        window.addEventListener('unhandledrejection', function(event) {
            console.warn('?? Promise rejetée non gérée:', event.reason);
            window.blazorErrorHandler.handleError(event.reason, {
                type: 'unhandledrejection'
            });
            event.preventDefault(); // Empêcher l'affichage de l'erreur par défaut
        });

        // Améliorer le bouton "Recharger" de la barre d'erreur
        document.addEventListener('DOMContentLoaded', function() {
            const reloadButton = document.querySelector('#blazor-error-ui .reload');
            if (reloadButton) {
                reloadButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    console.log('?? Rechargement demandé par l\'utilisateur');
                    window.location.reload();
                });
            }

            const dismissButton = document.querySelector('#blazor-error-ui .dismiss');
            if (dismissButton) {
                dismissButton.addEventListener('click', function() {
                    const errorUI = document.getElementById('blazor-error-ui');
                    if (errorUI) {
                        errorUI.style.display = 'none';
                    }
                });
            }
        });

        console.log('? Gestionnaire d\'erreurs JavaScript initialisé');
    }
};

// Initialiser automatiquement
window.blazorErrorHandler.initialize();