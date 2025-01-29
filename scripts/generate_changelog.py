import os
import openai
import subprocess
import tiktoken  # New import for tokenization

def get_latest_committed_cs_diffs():
    try:
        # Get the repository root directory
        repo_root_result = subprocess.run(
            ["git", "rev-parse", "--show-toplevel"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        if repo_root_result.returncode != 0:
            print("Error: Not a Git repository or Git is not available.")
            return ""
        repo_root = repo_root_result.stdout.strip()
        os.chdir(repo_root)  # Change to repo root to ensure correct pathspec matching

        # Get the latest commit modifying any .cs file
        latest_commit_result = subprocess.run(
            ["git", "log", "-1", "--pretty=format:%H", "--", "*.cs"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        latest_commit = latest_commit_result.stdout.strip()
        
        if not latest_commit:
            print("No recent commits modifying .cs files found.")
            return ""

        # Get the parent of the latest commit
        parent_commit_result = subprocess.run(
            ["git", "rev-parse", f"{latest_commit}^"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        if parent_commit_result.returncode != 0:
            print(f"Error finding parent commit: {parent_commit_result.stderr.strip()}")
            return ""
        parent_commit = parent_commit_result.stdout.strip()

        # Generate diff between parent and latest commit for .cs files
        diff_result = subprocess.run(
            ["git", "diff", parent_commit, latest_commit, "--", "*.cs"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )

        if diff_result.returncode != 0:
            raise Exception(f"Git error: {diff_result.stderr.strip()}")

        diffs = diff_result.stdout.strip()
        return diffs if diffs else "No significant changes detected in .cs files."
    
    except FileNotFoundError:
        print("Error: Git is not installed or not available in PATH.")
        return ""
    except Exception as e:
        print(f"Error fetching .cs file diffs: {e}")
        return ""

def generate_changelog(diffs):
    if not diffs or diffs.strip() == "No significant changes detected in .cs files.":
        print("No meaningful diffs to process.")
        return "No significant changes detected."

    try:
        openai.api_key = os.getenv("OPENAI_API_KEY")
        if not openai.api_key:
            raise Exception("OpenAI API key not found. Ensure OPENAI_API_KEY is set in the environment.")

        # Prepare the prompt
        prompt = f"""
        Generate a concise, categorized changelog based on the following C# code changes:
        {diffs}

        Focus only on actual code changes. Ignore comments, formatting adjustments, and unrelated modifications.

        Format the changelog with these categories:
        - Added: For new features or additions.
        - Fixed: For bug fixes or issue resolutions.
        - Updated: For changes or improvements.
        - Removed: For features or code that were removed.

        Use Markdown formatting with bullet points, headers for notable features, etc.
        Reduce specific jargon, elaborate on more complex features to the best of your knowledge.
        """

        # ChatCompletion messages structure
        messages = [
            {"role": "system", "content": "You are a helpful assistant that generates professional and categorized changelogs."},
            {"role": "user", "content": prompt.strip()}
        ]

        # Calculate approximate number of tokens using tiktoken
        model_name = "gpt-4o-mini"  # The model you plan to use
        encoding = tiktoken.encoding_for_model(model_name)
        
        total_tokens = 0
        for m in messages:
            # Add the tokens for each message's content
            total_tokens += len(encoding.encode(m["content"]))

        # Print the approximate token count
        print(f"Approximate number of tokens in prompt: {total_tokens}")

        response = openai.chat.completions.create(
            model=model_name,
            messages=messages,
            max_tokens=4096,
            temperature=0.7,
        )
        return response.choices[0].message.content.strip()
    except Exception as e:
        print(f"Error generating changelog: {e}")
        return "Failed to generate changelog."

def main():
    diffs = get_latest_committed_cs_diffs()
    if not diffs:
        print("No diffs found to process.")
        return

    changelog = generate_changelog(diffs)
    print("Generated Changelog:\n")
    print(changelog)

    try:
        with open("CHANGELOG.md", "a", encoding="utf-8") as changelog_file:
            changelog_file.write("\n## [Latest Changes]\n")
            changelog_file.write(changelog + "\n")
        print("Changelog successfully written to CHANGELOG.md")
    except Exception as e:
        print(f"Error writing changelog to file: {e}")

if __name__ == "__main__":
    main()
