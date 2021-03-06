# EmacsLikeKeyBindings FlashDevelop Plugin
[![Build status](https://ci.appveyor.com/api/projects/status/coe6jfbt87r8s1g6?svg=true)](https://ci.appveyor.com/project/lpubsppop01/emacslikekeybindingsfdplugin)

A FlashDevelop plugin that provides Emacs like key bindings.

## Supported Keys
| Keys            | Command                |
|-----------------|------------------------|
| Ctrl-f          | forward-char           |
| Ctrl-b          | backward-char          |
| Ctrl-a          | move-beginning-of-line |
| Ctrl-e          | move-end-of-line       |
| Ctrl-n          | next-line              |
| Ctrl-p          | previous-line          |
| Ctrl-d          | delete-char            |
| Ctrl-h          | delete-backward-char   |
| Ctrl-w          | kill-region            |
| Ctrl-k          | kill-line              |
| Ctrl-y          | yank                   |
| Ctrl-Space      | set-mark-command       |
| Ctrl-@          | set-mark-command       |
| Ctrl-g          | keyboard-quit          |
| Ctrl-/          | undo                   |
| Alt-/           | dabbrev-expand         |
| Ctrl-x Ctrl-f   | find-file              |
| Ctrl-s          | isearch-forward        |
| Ctrl-r          | isearch-backward       |

Notes:
- Not support kill-ring
- Yank replace a region

## Download
- [Latest Build for FlashDevelop 5.2.0 / .NET Framework 3.5 - AppVeyor](https://ci.appveyor.com/api/projects/lpubsppop01/emacslikekeybindingsfdplugin/artifacts/lpubsppop01.EmacsLikeKeyBindingsFDPlugin.fdz?job=Environment%3A%20FLASH_DEVELOP_VERSION%3D5.2.0)
- [Latest Build for FlashDevelop 5.3.0 / .NET Framework 4 - AppVeyor](https://ci.appveyor.com/api/projects/lpubsppop01/emacslikekeybindingsfdplugin/artifacts/lpubsppop01.EmacsLikeKeyBindingsFDPlugin.fdz?job=Environment%3A%20FLASH_DEVELOP_VERSION%3D5.3.0)


## Author
[lpubsppop01](https://github.com/lpubsppop01)

## License
[zlib License](https://github.com/lpubsppop01/EmacsLikeKeyBindingsFDPlugin/raw/master/LICENSE.txt)
