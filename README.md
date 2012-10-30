Wizards-or-whatever
===================

### Game project for COMP 3903
-----------
# Please refer to this section if you have commit issues
-----------
1. Go here and follow Windows instructions for **P4Merge: Visual Merge Tool**: http://www.perforce.com/downloads/complete_list
2. During installation, in Select Features, ensure that only **Visual Merge Tool (P4Merge)** is selected; all others should be X'd out
3. Navigate to the root directory of your local repo and edit your **.gitconfig** file to include the following:  
```    
    [merge] 

    tool = p4merge  
    [mergetool "p4merge"]  
    path = c:/Program Files/Perforce/p4merge.exe  
    trustExitCode = true
```
4. Save and exit, it should now be working. The command to merge files is:  
```git mergetool```

**Note:** The screen at the bottom is your result, so when you are happy with the changes, click save and it will commit that file for you  
**Note2:** Sometimes the .orig file will be held on to, this can be thrown away. Navigate to the file location after merging and delete it

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
