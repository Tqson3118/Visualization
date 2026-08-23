"""
package_usb.py — Export a clean, organized submission folder structure for USB
"""

import os
import sys
import shutil
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent.parent
DIST_USB = ROOT_DIR / "dist_usb"

def package():
    print(f"=== PACKAGING USB SUBMISSION DIRECTORY: {DIST_USB} ===")
    if DIST_USB.exists():
        shutil.rmtree(DIST_USB)
    DIST_USB.mkdir(parents=True, exist_ok=True)

    # 1. 01_Bao_Cao
    dir_baocao = DIST_USB / "01_Bao_Cao"
    dir_baocao.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT_DIR / "tailieu" / "BaoCaoDoAn.docx", dir_baocao / "BaoCaoDoAn.docx")
    shutil.copy2(ROOT_DIR / "tailieu" / "PRO2192_Project plan.xlsx", dir_baocao / "PRO2192_Project plan.xlsx")
    shutil.copy2(ROOT_DIR / "tailieu" / "PRO2192_Test cases.xlsx", dir_baocao / "PRO2192_Test cases.xlsx")
    shutil.copy2(ROOT_DIR / "tailieu" / "test_results.md", dir_baocao / "test_results.md")
    shutil.copy2(ROOT_DIR / "tailieu" / "test_results.json", dir_baocao / "test_results.json")
    print("  [01_Bao_Cao] Copied Word report, Excel sheets, and test results.")

    # 2. 02_Database
    dir_db = DIST_USB / "02_Database"
    dir_db.mkdir(parents=True, exist_ok=True)
    if (ROOT_DIR / "tailieu" / "database" / "VisualizationDSA_Backup.bak").exists():
        shutil.copy2(ROOT_DIR / "tailieu" / "database" / "VisualizationDSA_Backup.bak", dir_db / "VisualizationDSA_Backup.bak")
    shutil.copy2(ROOT_DIR / "tailieu" / "diagrams" / "dsa-visual-schema.dbml", dir_db / "dsa-visual-schema.dbml")
    shutil.copy2(ROOT_DIR / "tailieu" / "database" / "Huong_Dan_Restore_DB.md", dir_db / "Huong_Dan_Restore_DB.md")
    print("  [02_Database] Copied database .bak backup, DBML schema, and restore guide.")

    # 3. 03_Source_Code
    dir_src = DIST_USB / "03_Source_Code"
    dir_src.mkdir(parents=True, exist_ok=True)
    
    # Copy backend (ignore bin, obj, .vs)
    print("  [03_Source_Code] Copying backend...")
    shutil.copytree(
        ROOT_DIR / "source" / "VisualizationDSA" / "backend",
        dir_src / "backend",
        ignore=shutil.ignore_patterns("bin", "obj", ".vs", "*.user", "*.db", "node_modules")
    )

    # Copy frontend (ignore node_modules, dist, .vite)
    print("  [03_Source_Code] Copying frontend...")
    shutil.copytree(
        ROOT_DIR / "frontend",
        dir_src / "frontend",
        ignore=shutil.ignore_patterns("node_modules", "dist", ".vite", ".output")
    )

    # 4. 04_Huong_Dan_Cai_Dat
    dir_hd = DIST_USB / "04_Huong_Dan_Cai_Dat"
    dir_hd.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT_DIR / "tailieu" / "HUONG_DAN_CHAY_DU_AN.md", dir_hd / "HUONG_DAN_CHAY_DU_AN.md")
    shutil.copy2(ROOT_DIR / "docker-compose.yml", dir_hd / "docker-compose.yml")
    print("  [04_Huong_Dan_Cai_Dat] Copied startup guide and docker-compose.yml.")

    # Top-level Readme
    shutil.copy2(ROOT_DIR / "tailieu" / "HUONG_DAN_CHAY_DU_AN.md", DIST_USB / "README.md")

    print(f"\nSUCCESS: USB directory prepared at: {DIST_USB}")

if __name__ == "__main__":
    package()
