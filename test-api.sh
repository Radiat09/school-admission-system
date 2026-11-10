#!/usr/bin/env bash
set -e
BASE=http://localhost:8080
SID="20152691234567890"

echo "== CREATE CLASS =="
curl -s -X POST "$BASE/classes" -H 'Content-Type: application/json' -d '{"name":"Class 6 - Morning Shift","totalSeats":120}'
echo

echo "== GET CLASSES =="
curl -s "$BASE/classes"
echo

echo "== GET CLASS (missing) =="
curl -s -o /dev/null -w "status:%{http_code}\n" "$BASE/classes/nope"

echo "== APPLY =="
curl -s -X POST "$BASE/classes/0/holds" -H "X-Student-Id: $SID" 
echo
echo "(using real class id below)"
CLASS_ID=$(curl -s "$BASE/classes" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])" 2>/dev/null || echo "")
echo "class id: $CLASS_ID"
if [ -n "$CLASS_ID" ]; then
  curl -s -X POST "$BASE/classes/$CLASS_ID/holds" -H "X-Student-Id: $SID"
  echo
  echo "== APPLY DUPLICATE (same student, same class) =="
  curl -s -o /dev/null -w "status:%{http_code}\n" -X POST "$BASE/classes/$CLASS_ID/holds" -H "X-Student-Id: $SID"
  echo "== GET CLASSES (after hold) =="
  curl -s "$BASE/classes"
  echo
fi

echo "== MISSING STUDENT HEADER =="
curl -s -o /dev/null -w "status:%{http_code}\n" -X POST "$BASE/classes/$CLASS_ID/holds" 2>/dev/null

echo "== INVALID STUDENT ID =="
curl -s -o /dev/null -w "status:%{http_code}\n" -X POST "$BASE/classes/$CLASS_ID/holds" -H "X-Student-Id: abc" 2>/dev/null
