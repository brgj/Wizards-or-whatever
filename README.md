Wizards-or-whatever
===================

### Game project for COMP 3903

## Useful Github commands

```git status``` - Shows any uncommitted changes that you have on your local repository

```git stash``` - Store uncommitted changes on a stack so you can pull from upstream without committing

```git stash pop``` - Pop last stashed item

```git reset --hard HEAD``` - Resets all uncommitted work to the last check-in

```git checkout <file>``` - Resets a specific file to the last check-in

When a file is not being merged properly, to ensure your own copy is kept:   
```git add <file>``` - Keeps your local copy instead of attempting to merge differences between local and upstream

```git rebase --continue``` - Continue with synchronizing

If synchronizing does not work, this will force your changes upstream   
**Only use if you're sure your changes are correct**   
```git push --force origin master```

## Use this to ignore non-source files in your git repo

```git config --global core.excludesfile <path to local repo>/.gitignore```

## Download perforce to get merge working

http://www.perforce.com/downloads

Call this to merge problematic files and open perforce:   
```git mergetool```