# ?? Quick Health Check Commands

Automated monitoring commands for external tools like AWS Amplify, Azure Monitor, or custom monitoring solutions.

---

## ?? Database Health Checks

### Token Health Check
**Purpose:** Ensure no token accumulation (should always be ? 1 token per user)

```sql
-- Check for users with multiple tokens (CRITICAL ALERT if > 0)
SELECT COUNT(*) as critical_users_with_multiple_tokens
FROM (
    SELECT "UserId"
    FROM "RefreshTokens"
    GROUP BY "UserId"
    HAVING COUNT(*) > 1
) multi_token_users;
-- EXPECTED: 0
-- ALERT THRESHOLD: > 0
-- SEVERITY: HIGH
```

### Total Token Count
**Purpose:** Monitor token table growth

```sql
-- Total active tokens
SELECT COUNT(*) as total_active_tokens
FROM "RefreshTokens";
-- EXPECTED: ? number of active users
-- ALERT THRESHOLD: > 1000 (adjust based on user base)
-- SEVERITY: MEDIUM
```

### Active Sessions vs Users
**Purpose:** Detect abnormal login patterns

```sql
SELECT 
    (SELECT COUNT(*) FROM "AspNetUsers") as total_users,
    (SELECT COUNT(*) FROM "RefreshTokens") as active_sessions,
    ROUND(
        (SELECT COUNT(*)::DECIMAL FROM "RefreshTokens") / 
        NULLIF((SELECT COUNT(*)::DECIMAL FROM "AspNetUsers"), 0) * 100, 
        2
    ) as session_to_user_ratio_percent;
-- EXPECTED: ratio between 20-80%
-- ALERT THRESHOLD: > 90% (most users logged in - unusual)
-- SEVERITY: LOW
```

---

## ?? Payment System Health

### Payment Method Availability
**Purpose:** Ensure all payment methods are configured

```sql
-- Check payment methods exist
SELECT COUNT(*) as payment_methods_count
FROM "PaymentMethods";
-- EXPECTED: 4 (Credit Card, PayPal, Cash on Delivery, Bank Transfer)
-- ALERT THRESHOLD: < 4
-- SEVERITY: CRITICAL
```

### Recent Order Success Rate
**Purpose:** Monitor payment failures

```sql
-- Orders in last 24 hours
SELECT 
    COUNT(*) FILTER (WHERE "Status" = 'Paid') as successful_orders,
    COUNT(*) FILTER (WHERE "Status" = 'PaymentFailed') as failed_orders,
    COUNT(*) as total_orders,
    ROUND(
        COUNT(*) FILTER (WHERE "Status" = 'Paid')::DECIMAL / 
        NULLIF(COUNT(*)::DECIMAL, 0) * 100, 
        2
    ) as success_rate_percent
FROM "Orders"
WHERE "CreatedOn" > NOW() - INTERVAL '24 hours';
-- EXPECTED: success_rate > 85%
-- ALERT THRESHOLD: < 75%
-- SEVERITY: HIGH
```

### Payment Method Distribution
**Purpose:** Detect payment method issues

```sql
-- Orders by payment method in last 7 days
SELECT 
    "PaymentMethod",
    COUNT(*) as order_count,
    ROUND(AVG("TotalAmount"), 2) as avg_order_value
FROM "Orders"
WHERE "CreatedOn" > NOW() - INTERVAL '7 days'
GROUP BY "PaymentMethod"
ORDER BY order_count DESC;
-- EXPECTED: All payment methods should have some usage
-- ALERT THRESHOLD: Any method with 0 orders for 7+ days
-- SEVERITY: MEDIUM
```

### Failed Payments Analysis
**Purpose:** Identify failing payment patterns

```sql
-- Recent failed payments
SELECT 
    "PaymentMethod",
    COUNT(*) as failure_count,
    MAX("CreatedOn") as last_failure
FROM "Orders"
WHERE "Status" = 'PaymentFailed'
  AND "CreatedOn" > NOW() - INTERVAL '24 hours'
GROUP BY "PaymentMethod"
ORDER BY failure_count DESC;
-- EXPECTED: < 5 failures per payment method per day
-- ALERT THRESHOLD: > 10 failures for any method in 24h
-- SEVERITY: HIGH
```

---

## ??? Database Connection Health

### Connection Test
**Purpose:** Verify database is reachable

```sql
SELECT 1 as connection_ok;
-- EXPECTED: 1
-- ALERT THRESHOLD: No response or error
-- SEVERITY: CRITICAL
```

### Table Existence Check
**Purpose:** Ensure critical tables exist

```sql
SELECT 
    COUNT(*) as critical_tables_count
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    'Orders', 
    'OrderLines', 
    'PaymentMethods', 
    'RefreshTokens', 
    'AspNetUsers'
  );
-- EXPECTED: 5
-- ALERT THRESHOLD: < 5
-- SEVERITY: CRITICAL
```

### Database Size Monitoring
**Purpose:** Monitor database growth

```sql
SELECT 
    pg_size_pretty(pg_database_size(current_database())) as database_size,
    pg_database_size(current_database()) as size_bytes;
-- EXPECTED: Gradual growth
-- ALERT THRESHOLD: > 10GB (adjust based on needs)
-- SEVERITY: MEDIUM
```

---

## ?? Performance Metrics

### Recent Order Volume
**Purpose:** Detect traffic anomalies

```sql
-- Orders per hour (last 24 hours)
SELECT 
    DATE_TRUNC('hour', "CreatedOn") as hour,
    COUNT(*) as order_count
FROM "Orders"
WHERE "CreatedOn" > NOW() - INTERVAL '24 hours'
GROUP BY DATE_TRUNC('hour', "CreatedOn")
ORDER BY hour DESC
LIMIT 24;
-- EXPECTED: Normal traffic pattern
-- ALERT THRESHOLD: 10x deviation from average
-- SEVERITY: MEDIUM
```

### Average Order Processing Time
**Purpose:** Monitor checkout performance

```sql
-- Average order creation (can be extended with timestamps)
SELECT 
    DATE_TRUNC('hour', "CreatedOn") as hour,
    COUNT(*) as orders,
    AVG("TotalAmount") as avg_amount
FROM "Orders"
WHERE "CreatedOn" > NOW() - INTERVAL '6 hours'
GROUP BY DATE_TRUNC('hour', "CreatedOn")
ORDER BY hour DESC;
-- EXPECTED: Consistent processing
-- ALERT THRESHOLD: Sudden drops in order count
-- SEVERITY: MEDIUM
```

---

## ?? User Authentication Health

### Email Confirmation Rate
**Purpose:** Monitor user activation

```sql
SELECT 
    COUNT(*) FILTER (WHERE "EmailConfirmed" = true) as confirmed_users,
    COUNT(*) FILTER (WHERE "EmailConfirmed" = false) as unconfirmed_users,
    COUNT(*) as total_users,
    ROUND(
        COUNT(*) FILTER (WHERE "EmailConfirmed" = true)::DECIMAL / 
        NULLIF(COUNT(*)::DECIMAL, 0) * 100, 
        2
    ) as confirmation_rate_percent
FROM "AspNetUsers";
-- EXPECTED: > 70% confirmation rate
-- ALERT THRESHOLD: < 50%
-- SEVERITY: LOW
```

### Inactive Tokens (Expired Check)
**Purpose:** Monitor expired tokens (requires CreatedAt column)

```sql
-- Note: This query requires the CreatedAt column enhancement
-- See REFRESH_TOKEN_CLEANUP_GUIDE.md for implementation

-- SELECT COUNT(*) as expired_tokens
-- FROM "RefreshTokens"
-- WHERE "ExpiresAt" < NOW();
-- EXPECTED: 0 (with cleanup service running)
-- ALERT THRESHOLD: > 100
-- SEVERITY: LOW

-- Current workaround (until expiration is implemented):
SELECT 
    COUNT(*) as total_tokens,
    CASE 
        WHEN COUNT(*) > (SELECT COUNT(*) FROM "AspNetUsers") * 1.5 
        THEN 'ALERT: Possible stale tokens'
        ELSE 'OK'
    END as status
FROM "RefreshTokens";
```

---

## ?? Security Monitoring

### Failed Login Attempts
**Purpose:** Detect brute force attacks (requires logging)

```sql
-- This requires login attempt tracking
-- Placeholder for future implementation

-- SELECT 
--     COUNT(*) as failed_attempts,
--     "IpAddress",
--     MAX("AttemptTime") as last_attempt
-- FROM "LoginAttempts"
-- WHERE "Success" = false
--   AND "AttemptTime" > NOW() - INTERVAL '1 hour'
-- GROUP BY "IpAddress"
-- HAVING COUNT(*) > 5
-- ORDER BY failed_attempts DESC;
-- EXPECTED: 0 rows (no brute force)
-- ALERT THRESHOLD: > 10 attempts from single IP in 1h
-- SEVERITY: HIGH
```

### Recent User Registrations
**Purpose:** Detect unusual signup patterns

```sql
SELECT 
    DATE(u."Id"::text::timestamp) as registration_date,
    COUNT(*) as new_users
FROM "AspNetUsers" u
WHERE u."Id"::text::timestamp > NOW() - INTERVAL '7 days'
GROUP BY DATE(u."Id"::text::timestamp)
ORDER BY registration_date DESC;
-- EXPECTED: Steady growth
-- ALERT THRESHOLD: 10x spike in single day
-- SEVERITY: MEDIUM
```

---

## ?? API Health Endpoints

### Health Check Endpoint
**Purpose:** Verify API is responding

```bash
# HTTP Request
curl -f https://localhost:7094/health || echo "API DOWN"

# Expected: HTTP 200 OK
# Alert Threshold: HTTP 500 or timeout
# Severity: CRITICAL
```

### API Response Time
**Purpose:** Monitor API performance

```bash
# Measure response time (bash)
time curl -s https://localhost:7094/api/payment/methods > /dev/null

# Expected: < 500ms
# Alert Threshold: > 2000ms
# Severity: MEDIUM
```

---

## ?? Combined Health Dashboard Query

**Purpose:** Single query for overall system health

```sql
SELECT 
    -- Token Health
    (SELECT COUNT(*) FROM (
        SELECT "UserId" FROM "RefreshTokens" 
        GROUP BY "UserId" HAVING COUNT(*) > 1
    ) multi) as users_with_duplicate_tokens,
    
    -- Payment Methods
    (SELECT COUNT(*) FROM "PaymentMethods") as payment_methods_count,
    
    -- Recent Orders
    (SELECT COUNT(*) FROM "Orders" 
     WHERE "CreatedOn" > NOW() - INTERVAL '24 hours') as orders_last_24h,
    
    -- Success Rate
    ROUND(
        (SELECT COUNT(*)::DECIMAL FROM "Orders" 
         WHERE "Status" = 'Paid' 
         AND "CreatedOn" > NOW() - INTERVAL '24 hours') / 
        NULLIF((SELECT COUNT(*)::DECIMAL FROM "Orders" 
                WHERE "CreatedOn" > NOW() - INTERVAL '24 hours'), 0) * 100,
        2
    ) as success_rate_24h_percent,
    
    -- Active Users
    (SELECT COUNT(*) FROM "RefreshTokens") as active_sessions,
    
    -- Total Users
    (SELECT COUNT(*) FROM "AspNetUsers") as total_users,
    
    -- Database Size
    pg_size_pretty(pg_database_size(current_database())) as db_size;

-- Expected values:
-- users_with_duplicate_tokens: 0
-- payment_methods_count: 4
-- orders_last_24h: > 0 (if traffic expected)
-- success_rate_24h_percent: > 85
-- active_sessions: 20-80% of total_users
-- total_users: Growing
-- db_size: < 10GB (adjust as needed)
```

---

## ?? Alert Configuration Template

### Critical Alerts (Immediate Action)
- Database connection failure
- Payment methods count < 4
- Users with duplicate tokens > 0
- API health check failure
- Payment success rate < 50%

### High Priority Alerts (1-hour SLA)
- Payment success rate < 75%
- Failed payments > 10 for any method in 24h
- API response time > 3s

### Medium Priority Alerts (4-hour SLA)
- Token count > expected users * 1.5
- Database size > threshold
- Payment method with 0 orders for 7 days
- 10x spike in order volume

### Low Priority Alerts (24-hour SLA)
- Email confirmation rate < 50%
- Session to user ratio > 90%
- Stale tokens > 100

---

## ?? Integration Examples

### AWS CloudWatch (bash script)
```bash
#!/bin/bash
# health-check.sh

# Run health query
RESULT=$(psql -U postgres -d blazorshop -t -c "
    SELECT COUNT(*) FROM (
        SELECT \"UserId\" FROM \"RefreshTokens\" 
        GROUP BY \"UserId\" HAVING COUNT(*) > 1
    ) multi;
")

# Send to CloudWatch
aws cloudwatch put-metric-data \
    --namespace "BlazorShop/Tokens" \
    --metric-name "DuplicateTokenUsers" \
    --value "$RESULT" \
    --unit Count

# Alert if > 0
if [ "$RESULT" -gt 0 ]; then
    aws sns publish \
        --topic-arn "arn:aws:sns:region:account:alerts" \
        --message "CRITICAL: $RESULT users with duplicate tokens"
fi
```

### Azure Monitor (PowerShell)
```powershell
# health-check.ps1

# Run health query
$result = psql -U postgres -d blazorshop -t -c @"
    SELECT COUNT(*) FROM PaymentMethods;
"@

# Send to Azure Monitor
az monitor metrics list `
    --resource /subscriptions/.../blazorshop-api `
    --metric PaymentMethodsCount `
    --interval PT1M

# Alert if < 4
if ($result -lt 4) {
    Write-Error "CRITICAL: Only $result payment methods configured"
    exit 1
}
```

### Prometheus Exporter Format
```python
# metrics.py
import psycopg2

def get_metrics():
    conn = psycopg2.connect("dbname=blazorshop user=postgres")
    cur = conn.cursor()
    
    # Token health
    cur.execute("""
        SELECT COUNT(*) FROM (
            SELECT "UserId" FROM "RefreshTokens" 
            GROUP BY "UserId" HAVING COUNT(*) > 1
        ) multi
    """)
    duplicate_tokens = cur.fetchone()[0]
    
    # Payment success rate
    cur.execute("""
        SELECT 
            COUNT(*) FILTER (WHERE "Status" = 'Paid')::FLOAT / 
            NULLIF(COUNT(*)::FLOAT, 0)
        FROM "Orders"
        WHERE "CreatedOn" > NOW() - INTERVAL '1 hour'
    """)
    success_rate = cur.fetchone()[0] or 0
    
    return f"""
# HELP blazorshop_duplicate_tokens Users with duplicate refresh tokens
# TYPE blazorshop_duplicate_tokens gauge
blazorshop_duplicate_tokens {duplicate_tokens}

# HELP blazorshop_payment_success_rate Payment success rate (0-1)
# TYPE blazorshop_payment_success_rate gauge
blazorshop_payment_success_rate {success_rate}
"""
```

---

## ?? Monitoring Schedule

| Check | Frequency | Tool |
|-------|-----------|------|
| **Token Health** | Every 5 minutes | Automated |
| **Payment Success Rate** | Every 15 minutes | Automated |
| **Database Connection** | Every 1 minute | Automated |
| **API Health** | Every 1 minute | Automated |
| **Payment Method Count** | Every 1 hour | Automated |
| **Order Volume** | Every 1 hour | Automated |
| **Database Size** | Daily | Automated |
| **Security Review** | Weekly | Manual |

---

## ?? Quick Manual Check Commands

```bash
# Full system health check
psql -U postgres -d blazorshop << 'EOF'
\echo '=== SYSTEM HEALTH REPORT ==='
\echo ''
\echo '1. Token Health:'
SELECT COUNT(*) as duplicate_token_users FROM (
    SELECT "UserId" FROM "RefreshTokens" 
    GROUP BY "UserId" HAVING COUNT(*) > 1
) multi;

\echo ''
\echo '2. Payment Methods:'
SELECT COUNT(*) as payment_methods FROM "PaymentMethods";

\echo ''
\echo '3. Last 24h Orders:'
SELECT 
    COUNT(*) as total,
    COUNT(*) FILTER (WHERE "Status" = 'Paid') as paid,
    COUNT(*) FILTER (WHERE "Status" = 'PaymentFailed') as failed
FROM "Orders"
WHERE "CreatedOn" > NOW() - INTERVAL '24 hours';

\echo ''
\echo '4. Active Sessions:'
SELECT COUNT(*) as active_sessions FROM "RefreshTokens";

\echo ''
\echo '5. Database Size:'
SELECT pg_size_pretty(pg_database_size(current_database()));
EOF
```

---

## ? Expected Output Examples

### Healthy System
```
=== SYSTEM HEALTH REPORT ===

1. Token Health:
 duplicate_token_users
-----------------------
                     0  ?

2. Payment Methods:
 payment_methods
-----------------
               4  ?

3. Last 24h Orders:
 total | paid | failed
-------+------+--------
    45 |   43 |      2  ? (95.5% success)

4. Active Sessions:
 active_sessions
-----------------
              23  ?

5. Database Size:
 pg_size_pretty
----------------
 127 MB         ?
```

### System with Issues
```
=== SYSTEM HEALTH REPORT ===

1. Token Health:
 duplicate_token_users
-----------------------
                     3  ? CRITICAL

2. Payment Methods:
 payment_methods
-----------------
               3  ? CRITICAL (missing 1 method)

3. Last 24h Orders:
 total | paid | failed
-------+------+--------
    50 |   30 |     20  ??  (60% success - LOW)

4. Active Sessions:
 active_sessions
-----------------
             150  ??  (High - possible token leak)

5. Database Size:
 pg_size_pretty
----------------
 12 GB          ??  (Near threshold)
```

---

## ?? Related Documentation

- `PAYMENT_COMMANDS_REFERENCE.md` - Full command reference
- `REFRESH_TOKEN_CLEANUP_GUIDE.md` - Token management details
- `PAYMENT_DIAGNOSTIC_GUIDE.md` - Troubleshooting guide

---

**Use these queries in your monitoring tool to maintain system health!** ???
