import json
import sys

def validate_dialogue(dialogue, valid_ids):
    """
    Validate a single dialogue's structure and check that all nextId and nextScene references exist.
    """
    required_keys = {"id", "background", "nextScene", "lines"}
    missing = required_keys - dialogue.keys()
    if missing:
        return False, f"Brakuje kluczy: {', '.join(missing)}"

    # Check nextScene reference if not empty
    next_scene = dialogue.get("nextScene", "")
    if next_scene and next_scene not in valid_ids:
        return False, f"nextScene '{next_scene}' nie istnieje w zestawie id"

    if not isinstance(dialogue["lines"], list):
        return False, "Pole 'lines' musi być listą"

    for i, line in enumerate(dialogue["lines"]):
        for key in ["speaker", "text", "sprite", "background"]:
            if key not in line:
                return False, f"Brakuje klucza '{key}' w linii {i}"

        if "battle" in line and not isinstance(line["battle"], str):
            return False, f"'battle' w linii {i} musi być stringiem"

        if "choices" in line:
            if not isinstance(line["choices"], list):
                return False, f"'choices' w linii {i} musi być listą"
            for j, choice in enumerate(line["choices"]):
                if not isinstance(choice, dict):
                    return False, f"Choice[{j}] w linii {i} nie jest słownikiem"
                if "text" not in choice or "nextId" not in choice:
                    return False, f"Brakuje 'text' lub 'nextId' w choice[{j}] w linii {i}"
                if choice["nextId"] not in valid_ids:
                    return False, f"nextId '{choice['nextId']}' w choice[{j}] w linii {i} nie istnieje"

    return True, "OK"

def main(json_path):
    try:
        with open(json_path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"Błąd składni JSON: {e}")
        return

    if "dialogues" not in data or not isinstance(data["dialogues"], list):
        print("Struktura pliku nie zawiera klucza 'dialogues' z listą")
        return

    dialogues = data["dialogues"]

    # Check for unique IDs
    ids = [dlg.get("id") for dlg in dialogues if "id" in dlg]
    dup_ids = {id for id in ids if ids.count(id) > 1}
    if dup_ids:
        print(f"Zduplikowane id: {', '.join(dup_ids)}")
        return

    valid_ids = set(ids)
    all_valid = True

    for dlg in dialogues:
        dlg_id = dlg.get("id", "<brak id>")
        valid, msg = validate_dialogue(dlg, valid_ids)
        if not valid:
            print(f"Dialog '{dlg_id}': {msg}")
            all_valid = False

    if all_valid:
        print("Wszystkie dialogi są poprawne, włącznie z nextId i nextScene.")
    else:
        print("Wykryto błędy w dialogach.")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Użycie: python validate_dialogues.py <ścieżka_do_pliku_json>")
    else:
        main(sys.argv[1])
