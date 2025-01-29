import os
import openai
import subprocess

# Fetch changed .cs file diffs from Git
def get_cs_file_diffs():
    try:
        # Get the list of changed .cs files in the last 10 commits
        changed_files_result = subprocess.run(
            ["git", "diff", "--name-only", "HEAD~10", "--", "*.cs"],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            errors="replace"
        )
        
        changed_files = changed_files_result.stdout.strip().split("\n")
        if not changed_files or changed_files[0] == "":
            print("No .cs file changes detected.")
            return ""

        # Get the actual diffs for the changed .cs files
        diff_result = subprocess.run(
            ["git", "diff", "-p", "--"] + changed_files,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            errors="replace"
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

# Use OpenAI API to generate a changelog
def generate_changelog(diffs):
    if not diffs or diffs.strip() == "No significant changes detected in .cs files.":
        print("No meaningful diffs to process.")
        return "No significant changes detected."

    try:
        # Set OpenAI API key
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

        Use Markdown formatting with bullet points.
        """

        # Call OpenAI's Chat API
        response = openai.ChatCompletion.create(
            model="gpt-4",
            messages=[
                {"role": "system", "content": "You are a helpful assistant that generates professional and categorized changelogs."},
                {"role": "user", "content": prompt.strip()}
            ],
            max_tokens=1500,  # Limit token usage to prevent exceeding API limits
            temperature=0.7,
        )
        return response.choices[0].message.content.strip()
    except Exception as e:
        print(f"Error generating changelog: {e}")
        return "Failed to generate changelog."

# Main function
def main():
    diffs = get_cs_file_diffs()
    if not diffs:
        print("No diffs found to process.")
        return

    changelog = generate_changelog(diffs)
    print("Generated Changelog:\n")
    print(changelog)

    # Save the changelog to CHANGELOG.md
    try:
        with open("CHANGELOG.md", "a", encoding="utf-8") as changelog_file:
            changelog_file.write("\n## [Latest Changes]\n")
            changelog_file.write(changelog + "\n")
        print("Changelog successfully written to CHANGELOG.md")
    except Exception as e:
        print(f"Error writing changelog to file: {e}")

if __name__ == "__main__":
    main()
