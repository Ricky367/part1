import random
import os

def main():
    # ANSI color codes
    BLUE = '\033[94m'
    GREEN = '\033[92m'
    RED = '\033[91m'
    YELLOW = '\033[93m'
    RESET = '\033[0m'

    fun_messages = [
        "✨ Math magic! ✨",
        "🎉 Calculation complete! 🎉",
        "😎 You're a math star! 😎",
        "🚀 Fast as lightning! 🚀",
        "👏 Well done! 👏"
    ]

    while True:
        try:
            os.system('cls' if os.name == 'nt' else 'clear')
        except Exception:
            pass  # Ignore errors if clear/cls is not available
        print(f"{BLUE}=== 🧮 Cool Calculator ==={RESET}")
        print(f"{YELLOW}1. ➕ Add")
        print("2. ➖ Subtract")
        print("3. ✖️ Multiply")
        print("4. ➗ Divide")
        print("5. ❌ Exit" + RESET)

        choice = input(f"{GREEN}Enter choice (1/2/3/4/5): {RESET}")

        if choice == '5':
            print(f"{RED}Goodbye! 👋{RESET}")
            break

        if choice in ('1', '2', '3', '4'):
            try:
                num1 = float(input("Enter first number: "))
                num2 = float(input("Enter second number: "))
            except ValueError:
                print(f"{RED}Please enter valid numbers!{RESET}")
                input("Press Enter to continue...")
                continue

            if choice == '1':
                print(f"{GREEN}{num1} + {num2} = {num1 + num2}{RESET}")
            elif choice == '2':
                print(f"{GREEN}{num1} - {num2} = {num1 - num2}{RESET}")
            elif choice == '3':
                print(f"{GREEN}{num1} * {num2} = {num1 * num2}{RESET}")
            elif choice == '4':
                if num2 != 0:
                    print(f"{GREEN}{num1} / {num2} = {num1 / num2}{RESET}")
                else:
                    print(f"{RED}Error: Cannot divide by zero.{RESET}")
            print(f"{YELLOW}{random.choice(fun_messages)}{RESET}")
        else:
            print(f"{RED}Invalid input{RESET}")

        input("Press Enter to continue...")

if __name__ == "__main__":
    main()