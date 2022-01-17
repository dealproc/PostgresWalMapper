setup the docker image:
docker network create pgsql
docker run --name some-postgres --network pgsql --publish 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d debezium/postgres:12

connect and create the tables & such (do this in psql console):
CREATE TABLE films (
    code        char(5) CONSTRAINT firstkey PRIMARY KEY,
    title       varchar(40) NOT NULL,
    did         integer NOT NULL,
    date_prod   date,
    kind        varchar(10),
    len         interval hour to minute
);

CREATE TABLE distributors (
     did    SERIAL PRIMARY KEY,
     name   varchar(40) NOT NULL CHECK (name <> '')
);

CREATE PUBLICATION films_pub FOR TABLE films, distributors;
SELECT * FROM pg_create_logical_replication_slot('films_slot', 'pgoutput');

INSERT INTO distributors (name) VALUES ('dist-1');
INSERT INTO distributors (name) VALUES ('dist-2');
INSERT INTO distributors (name) VALUES ('dist-3');
INSERT INTO distributors (name) VALUES ('dist-4');
INSERT INTO distributors (name) VALUES ('dist-5');

INSERT INTO films(code, title, did, date_prod, kind) VALUES('GOON', 'The Goonies', 1, '2020-01-01', 'kind');
INSERT INTO films(code, title, did, date_prod, kind) VALUES('GRM', 'Gremlins', 1, '2020-01-01', 'kind');
INSERT INTO films(code, title, did, date_prod, kind) VALUES('GRM2', 'Gremlins 2', 2, '2020-01-01', 'kind');
INSERT INTO films(code, title, did, date_prod, kind) VALUES('OS', 'Office Space', 3, '2020-01-01', 'kind');

UPDATE films SET title = 'Office Space 2' WHERE code = 'OS';
UPDATE films SET kind = '123';
DELETE FROM films WHERE code = 'GOON';

SELECT pg_drop_replication_slot('films_slot');
DROP PUBLICATION films_pub