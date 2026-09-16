# Roblox Shader Manager

Roblox Shader Manager est un utilitaire gaming sécurisé conçu pour installer, initialiser, activer et désactiver des shaders personnalisés pour Roblox. Son objectif principal est de garantir que l'utilisateur ne se fasse pas bannir par l'anti-cheat Byfron/Hyperion.

Pour ce faire, le logiciel utilise exclusivement la technologie NVIDIA Freestyle (dossier Ansel).

**Caractéristiques clés de sécurité :**
- **AUCUNE injection de mémoire** : Le programme ne s'attache pas au processus `RobloxPlayerBeta.exe`.
- **AUCUNE modification des fichiers du jeu** : Le programme ne modifie pas les dossiers d'installation de Roblox.
- Le logiciel agit uniquement en copiant, déplaçant et supprimant des fichiers de shaders dans le dossier officiel de NVIDIA (`C:\Program Files\NVIDIA Corporation\Ansel`).

## Architecture du Projet

Le projet a été développé en C# avec WPF (Windows Presentation Foundation) afin d'offrir une interface utilisateur moderne et d'assurer une parfaite intégration au système Windows, notamment en ce qui concerne l'élévation de privilèges via l'UAC.

Voici l'architecture du projet :

```
RobloxShaderManager/
├── RobloxShaderManager.csproj   # Fichier de projet .NET contenant la configuration et les dépendances.
├── app.manifest                 # Manifeste UAC exigeant les droits Administrateur.
├── App.xaml / App.xaml.cs       # Point d'entrée de l'application WPF.
├── MainWindow.xaml              # Interface utilisateur (XAML) sombre et moderne.
├── MainWindow.xaml.cs           # Logique C# (gestion des dossiers Ansel et Shaders_Pack).
└── Shaders_Pack/                # Dossier local (créé automatiquement) où placer vos fichiers .fx.
```

## Prérequis et Dépendances

- **Système d'Exploitation :** Windows 10 ou Windows 11.
- **SDK :** .NET 10.0 (ou version compatible) pour compiler l'application.
- **Carte Graphique :** Une carte graphique NVIDIA est recommandée, avec *GeForce Experience* ou la *NVIDIA App* installée.
- **Package NuGet :** `System.Management` (utilisé pour vérifier la présence de la carte graphique via WMI).

## Instructions de Compilation et de Lancement

1. **Cloner ou télécharger le dépôt.**
2. **Ouvrir un terminal (ou PowerShell) dans le dossier `RobloxShaderManager`.**
3. **Compiler l'application :**
   Exécutez la commande suivante :
   ```bash
   dotnet build
   ```
4. **Exécuter l'application :**
   Exécutez la commande suivante :
   ```bash
   dotnet run
   ```
   *Note : Sous Windows, l'application demandera automatiquement les droits d'administrateur via une fenêtre UAC à son lancement. Cela est indispensable pour écrire dans le dossier `Program Files`.*

5. **Déploiement pour l'utilisateur final (Exécutable autonome) :**
   Si vous souhaitez générer un seul fichier `.exe` à distribuer :
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

## Mode d'emploi

1. Lancez **RobloxShaderManager.exe** (acceptez la demande d'élévation de privilèges).
2. Au premier lancement, l'application va créer automatiquement un dossier nommé `Shaders_Pack` juste à côté de l'exécutable.
3. Placez vos fichiers de shaders personnalisés (au format `.fx`, type ReShade) et les dossiers de textures associés dans le dossier `Shaders_Pack`.
4. Cliquez sur le bouton **ACTIVER LES SHADERS** dans le logiciel.
5. Lancez Roblox. En jeu, appuyez sur la combinaison de touches `Alt + F3` pour ouvrir le menu NVIDIA Freestyle et sélectionnez vos filtres.
6. Une fois que vous avez terminé de jouer, cliquez sur **DÉSACTIVER LES SHADERS** pour que le logiciel retire proprement les fichiers du dossier NVIDIA.
