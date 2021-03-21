select * from accounts as a join users as u on a.user_id = u.user_id
--audit transfers
select t.transfer_id,tt.transfer_type_desc,ts.transfer_status_desc,af.username as sender,at.username as reciever,amount from 
	transfers as t join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id
	join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id
	join (select account_id,username from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id
	join (select account_id,username from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id

--match account ID to user name
select account_id,username from accounts as a join users as u on a.user_id = u.user_id

--

--get default acc ID for username
select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Ben' AND is_default_account = 1

--get transactions involing a user
declare @userId int;
set @userId = 4;

select * from users where user_id = @userId


select t.transfer_id,tt.transfer_type_desc,ts.transfer_status_desc,af.username as sender,at.username as reciever,amount from 
	transfers as t join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id
	join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id
	join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id
	join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id
	where af.user_id = @userId or at.user_id = @userId

select t.transfer_id,tt.transfer_type_desc,ts.transfer_status_desc,af.username as sender,at.username as reciever,amount from transfers as t join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id where af.user_id = @userId or at.user_id = @userId

--get transactions involing a user
declare @userId int;
set @userId = 4;

select * from 
	transfers as t join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id 
	join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id 
	join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id
	join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id
	where af.user_id = @userId or at.user_id = @userId

select * from transfers as t join transfer_types as tt on t.transfer_type_id = tt.transfer_type_id join transfer_statuses as ts on t.transfer_status_id = ts.transfer_status_id join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as af on t.account_from = af.account_id join (select account_id,username,a.user_id from accounts as a join users as u on a.user_id = u.user_id) as at on t.account_to = at.account_id where af.user_id = @userId or at.user_id = @userId