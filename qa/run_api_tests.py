import json
import urllib.request
import urllib.error
import time

BASE_URL = 'http://localhost:5000/api/v1'

def api(path, method='GET', data=None, token=None):
    url = BASE_URL + path
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = f'Bearer {token}'
    req_data = json.dumps(data).encode('utf-8') if data is not None else None
    req = urllib.request.Request(url, data=req_data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req) as response:
            status = response.status
            body = response.read().decode('utf-8')
            try:
                res_json = json.loads(body)
            except Exception:
                res_json = body
            return {'status': status, 'ok': True, 'data': res_json, 'text': body}
    except urllib.error.HTTPError as e:
        body = e.read().decode('utf-8')
        try:
            res_json = json.loads(body)
        except Exception:
            res_json = json
        return {'status': e.code, 'ok': False, 'data': res_json, 'text': body}
    except Exception as e:
        return {'status': 0, 'ok': False, 'data': str(e), 'text': str(e)}

FINDINGS_LIST = []

def log_finding(id_name, severity, title, category, url, steps, expected, actual, cause, fix):
    FINDINGS_LIST.append({
        'id': id_name,
        'severity': severity,
        'title': title,
        'category': category,
        'url': url,
        'steps': steps,
        'expected': expected,
        'actual': actual,
        'cause': cause,
        'fix': fix
    })
    print(f"[{severity}] {id_name} - {title}")


print('=== STARTING AUTOMATED AUDIT SYSTEM ===')
tokens = {}
# 1. Test Seed Logins
print('\n--- 1. Testing Seed Logins ---')
for role, email, pwd in [
    ('ADMIN', 'admin@system.local', 'Admin@123'),
    ('TEACHER', 'teacher@demo.local', 'Teacher@123'),
    ('STUDENT', 'student@demo.local', 'Student@123')
]:
    res = api('/auth/login', 'POST', {'email': email, 'password': pwd})
    print(f"Login {role}: status={res['status']}")
    if res['ok']:
        data_obj = res['data']
        tokens[role] = data_obj.get('accessToken') or data_obj.get('loginResponse', {}).get('accessToken')
    else:
        err_msg = res.get('text', '')
        log_finding('QA-AUTH-01', 'P0', f'Không đăng nhập được tài khoản seed {role}', 'chức năng', '/login', f'Đăng nhập với {email}', 'Đăng nhập thành công', f'Lỗi: {err_msg}', 'AuthController.cs', 'Kiểm tra mật khẩu và SeedPasswords')

# 2. Test RBAC Gating phía BE
print('\n--- 2. Testing RBAC Gating ---')
if 'STUDENT' in tokens:
    s_header = tokens['STUDENT']
    for name, ep, method in [
        ('Admin Stats', '/admin/stats', 'GET'),
        ('Admin Users', '/users', 'GET'),
        ('Admin BugReports', '/admin/bug-reports', 'GET'),
        ('Create Topic', '/topics', 'POST')
    ]:
        body_data = {'title': 'Test'} if method == 'POST' else None
        res = api(ep, method, data=body_data, token=s_header)
        print(f"RBAC Student -> {name} ({ep}): status={res['status']}")
        if res['status'] not in [401, 403]:
            log_finding('QA-RBAC-01', 'P1', f'Hở quyền BE: Student gọi được {ep}', 'phân quyền', ep, f'Student gọi {ep}', 'Bị chặn 401/403', f'Status là {res["status"]}', 'Authorize attribute missing', 'Gán [Authorize(Roles = "ADMIN")]')

# 3. Test Course State Machine & Moderation
print('\n--- 3. Testing Course State Machine ---')
if 'TEACHER' in tokens and 'ADMIN' in tokens:
    t_header = tokens['TEACHER']
    a_header = tokens['ADMIN']
    
    # Create Draft course
    create_res = api('/concepts/courses', 'POST', {
        'title': 'Khóa học Cấy Nhị Phân Test QA',
        'description': 'Mô tả khóa học test State Machine',
        'category': 'Tree',
        'difficulty': 'Intermediate',
        'scope': 'draft'
    }, token=t_header)
    course_id = create_res.get('data', {}).get('id')
    print(f'Create Draft Course: status={create_res["status"]}, id={course_id}')
    
    if course_id:
        # Check public visibility
        pub_1 = api('/concepts/courses', 'GET')
        is_visible = any(str(c.get('id')) == str(course_id) for c in (pub_1.get('data') or []))
        print(f'Draft Course visible on /path? {is_visible} (Expected: False)')
        if is_visible:
            log_finding('QA-PATH-01', 'P1', 'Lộ trình Draft lại hiển thị trên /path công khai', 'phân quyỆn/quản lý', '/path', '1) Tạo lộoi trình Draft. 2) Mở /path.', 'Không xuất hiện', 'Xuất hiện', 'ConceptsController.cs query không loại biệt Draft', 'Thêm diều kiện Status == Active và Visibility == Public')
        
        # Submit for review
        sub_pes = api(f'/concepts/courses/{course_id}/submit-review', 'POST', token=t_header)
        print(f'Submit Review: status={sub_pes["status"]}')
        
        # Admin approve
        app_res = api(f'/concepts/courses/{course_id}/review', 'POST', {
            'approve': True
        }, token=a_header)
        print(f'Admin Approve: status={app_res["status"]}')
        
        # Check public after approve
        pub_2 = api('/concepts/courses', 'GET')
        is_now_pub = any(str(c.get('id')) == str(course_id) for c in (pub_2.get('data') or []))
        print(f'Course visible after Approve? {is_now_pub} (Expected: True)')

# 4. Test Classes & Deadlines in Past
print('\n--- 4. Testing Classes & Deadlines ---')
if 'TEACHER' in tokens and 'STUDENT' in tokens:
    t_header = tokens['TEACHER']
    s_header = tokens['STUDENT']
    
    cls_res = api('/classes', 'POST', {
        'name': 'Lớp Test Deadline QA Playwright',
        'description': 'Lớp kiểm thử',
        'maxStudents': 25
    }, token=t_header)
    class_id = cls_res.get('data', {}).get('id')
    invite_code = cls_res.get('data', {}).get('inviteCode')
    print(f'Teacher Create Class: status={cls_res["status"]}, id={class_id}, code={invite_code}')
    
    if class_id and invite_code:
        # just test join by wrong code
        wrong_join = api('/classes/join-by-code', 'POST', {'inviteCode': 'XXYYZZ'}, token=s_header)
        print(f'ST Join Wrong Code: status={wrong_join["status"]}')
        
        # ST join right code
        right_join = api('/classes/join-by-code', 'POST', {'inviteCode': invite_code}, token=s_header)
        print(f'ST Join Right Code: status={right_join["status"]}')
        
        # ST Join again (double join)
        again_join = api('/classes/join-by-code', 'POST', {'inviteCode': invite_code}, token=s_header)
        print(f'ST Double Join: status={again_join["status"]} (Expected: 400 or 409)')
        
        # Set deadline in past
        dead_res = api(f'/classes/{class_id}/assignments/deadline', 'PUT', {
            'pathItemId': 1,
            'dueAt': '2020-01-01T00:00:00Z',
            'allowLateSubmission': True
        }, token=t_header)
        print(f'Set deadline in past: status={dead_res["status"]}')

# 5. Test Gamification & Quests & Shop
print('\n--- 5. Testing Gamification ---')
if 'STUDENT' in tokens:
    s_header = tokens['STUDENT']
    quests_res = api('/gamification/quests', 'GET', token=s_header)
    print(f'Quests List: status={quests_res["status"]}, count={len(quests_res.get("data") or [])}')
    
    shop_res = api('/gamification/shop', 'GET', token=s_header)
    print(f'Shop Items: status={shop_res["status"]}, count={len(shop_res.get("data") or [])}')


# Write Findings to FINDINGS.md
if FINDINGS_LIST:
    with open('qa/FINDINGS.md', 'a', encoding='utf-8') as f:
        for fitem in FINDINGS_LIST:
            fblock = f"### {fitem['id']} — [{fitem['severity']}] — {fitem['title']}\n- Loại: {fitem['category']}\n- Màn/URL: {fitem['url']}\n- Role: All\n- Bước tái hiện: {fitem['steps']}\n- Kỳ vọng vs Thực tế: {fitem['expected']} vs {fitem['actual']}\n- Bằng chứng: qa/evidence/{fitem['id']}.png\n- Suy đoán nguyên nhân: {fitem['cause']}\n- Gợi ý fix: {fitem['fix']}\n\n---\n\n"
            f.write(fblock)

print('\n=== AUTOMATED AUDIT FINISHED ===')
