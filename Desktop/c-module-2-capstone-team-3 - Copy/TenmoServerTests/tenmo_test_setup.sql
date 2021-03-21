
--delete from all tables
delete from transfers
delete from accounts
delete from users

--reseed the tables. TESTS SHOULD BE REPEATABLE
DBCC CHECKIDENT ('transfers', RESEED, 0)
DBCC CHECKIDENT ('accounts', RESEED, 0)
DBCC CHECKIDENT ('users', RESEED, 0)


--create users
INSERT INTO users(username, password_hash, salt)
	Values
		('Todd', 'c590IzEim+IucpPRNXXUxZLACFA=', 'yORn8ce0v/s='),--pw: sage
		('Paul', 'xOPYWf4ixQwNhGxfQ0+q+rMF5YU=', 'A99GYKJ/b8A='),--pw: pass
		('Mike', 'deA7OAx1xHRd1FdnP0NoP79zfqs=', '8b4G/VxvGk0='),--pw: ben
		('Ben' , 'Kih0YkuLPHM6uuqXrmxLRplkh7w=', 'N1vtTL81Mg4='),--pw: mike
		('Val' , 'NhAgWbJEgXVea+SiHID7eVHQgWY=', 'tDm2GZ/URa0=')--pw: jobs

--Create accounts
INSERT INTO accounts(user_id,balance, is_default_account)
	values
		((select user_id from users where username = 'Todd'),1000, 1),
		((select user_id from users where username = 'Paul'),1000, 1),
		((select user_id from users where username = 'Paul'),2000, 0),--savings
		((select user_id from users where username = 'Mike'),1000, 1),
		((select user_id from users where username = 'Ben'),775, 1),
		((select user_id from users where username = 'Val'),1225, 1)

--create transfers
INSERT INTO transfers(transfer_type_id,transfer_status_id,account_from,account_to,amount)
	values
	(
		(select transfer_type_id from transfer_types where transfer_type_desc = 'Send'),--transfer_type_id
		(select transfer_status_id from transfer_statuses where transfer_status_desc = 'Approved'),--transfer_status_id
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Ben' AND is_default_account = 1),--account_from
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Val' AND is_default_account = 1),--account_from
		(150)
	),
	(
		(select transfer_type_id from transfer_types where transfer_type_desc = 'Send'),--transfer_type_id
		(select transfer_status_id from transfer_statuses where transfer_status_desc = 'Approved'),--transfer_status_id
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Ben' AND is_default_account = 1),--account_from
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Val' AND is_default_account = 1),--account_from
		(100)
	),
	(
		(select transfer_type_id from transfer_types where transfer_type_desc = 'Send'),--transfer_type_id
		(select transfer_status_id from transfer_statuses where transfer_status_desc = 'Approved'),--transfer_status_id
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Val' AND is_default_account = 1),--account_from
		(select account_id from accounts as a join users as u on a.user_id = u.user_id where u.username='Ben' AND is_default_account = 1),--account_from
		(25)
	)